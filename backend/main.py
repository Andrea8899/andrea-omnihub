# backend/main.py
import os
from datetime import datetime
from typing import List, Dict, Optional
from fastapi import FastAPI, HTTPException, Depends, status
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from sqlalchemy import create_engine, Column, String, Boolean, Text, JSON, text
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker, Session

# ==========================================
# 1. CONFIGURAZIONE DATABASE CONFIG & ORM
# ==========================================
DATABASE_URL = "sqlite:///./strategies.db"

engine = create_engine(
    DATABASE_URL, connect_args={"check_same_thread": False}
)
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)
Base = declarative_base()

# Modello Tabella SQLite delle Strategie
class DBStrategy(Base):
    __tablename__ = "strategies"

    id = Column(String, primary_key=True, index=True)
    title = Column(String, nullable=False)
    created = Column(String, nullable=False)
    createdDisplay = Column(String, nullable=False)
    modified = Column(String, nullable=False)
    isSavedDB = Column(Boolean, default=True)
    isSavedGit = Column(Boolean, default=True)
    code = Column(Text, default="")
    blocks = Column(JSON, nullable=False) # Contiene entrata, uscita, stopLoss, ecc.

# Crea le tabelle nel Database se non esistono
Base.metadata.create_all(bind=engine)


# ==========================================
# 2. DIPENDENZA GET_DB
# ==========================================
def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()


# ==========================================
# 3. FASTAPI SCHEMAS (PYDANTIC)
# ==========================================
class StrategyBlocks(BaseModel):
    entrata: str = ""
    uscita: str = ""
    stopLoss: str = ""
    lottaggio: str = ""
    parzializzazione: str = ""
    trailingStop: str = ""

class StrategyCreate(BaseModel):
    id: str
    title: str
    code: str
    blocks: StrategyBlocks

class StrategyResponse(BaseModel):
    id: str
    title: str
    created: str
    createdDisplay: str
    modified: str
    isSavedDB: bool
    isSavedGit: bool
    code: str
    blocks: StrategyBlocks

    class Config:
        from_attributes = True


# ==========================================
# 4. INIZIALIZZAZIONE FASTAPI & CORS
# ==========================================
app = FastAPI(title="OmniHub Python Backend", version="1.0.0")

# Permetti la comunicazione tra React (Front-end) e FastAPI (Back-end)
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# ==========================================
# 5. ROTTE API DI CONTROLLO & HEALTH CHECK
# ==========================================
@app.get("/api/health")
def health_check():
    """Ritorna lo stato di connessione del Database Locale per l'indicatore nella Sidebar."""
    db = SessionLocal()
    try:
        # Usa text() per compatibilità con tutte le versioni di SQLAlchemy
        db.execute(text("SELECT 1"))
        return {"status": "connected"}
    except Exception as e:
        print(f"⚠️ Errore Health Check Database: {e}") # Questo ti stamperà sul terminale l'errore reale se fallisce
        return {"status": "disconnected"}
    finally:
        db.close()


# ==========================================
# 6. ROTTE API CRUD PER LE STRATEGIE
# ==========================================

@app.get("/api/strategies", response_model=List[StrategyResponse])
def get_strategies(db: Session = Depends(get_db)):
    """Ritorna l'elenco di tutte le strategie presenti nel Database."""
    strategies = db.query(DBStrategy).all()
    
    # Se il DB è vuoto, inseriamo un esempio iniziale di fallback
    if not strategies:
        now = datetime.now()
        default_strat = DBStrategy(
            id="1",
            title="SAMPLETREND_ROBOT",
            created=now.isoformat(),
            createdDisplay=now.strftime("%d/%m/%Y, %H:%M:%S"),
            modified=now.strftime("%d/%m/%Y, %H:%M:%S"),
            isSavedDB=True,
            isSavedGit=True,
            code="// Esempio cAlgo C#\nusing cAlgo.API;\n\nnamespace cAlgo\n{\n    [Robot(AccessRights = AccessRights.None)]\n    public class NewRobot : Robot\n    {\n        protected override void OnStart()\n        {\n            Print('OmniHub Connesso');\n        }\n    }\n}",
            blocks={
                "entrata": "Incrocio rialzista di due medie mobili sul grafico orario.",
                "uscita": "Incrocio ribassista opposto o target fisso.",
                "stopLoss": "Posizionato sotto l'ultimo minimo relativo dell'oscillazione.",
                "lottaggio": "Lotto fisso a 0.1 o 1% del capitale disponibile.",
                "parzializzazione": "Chiusura del 50% della posizione al raggiungimento del primo target.",
                "trailingStop": "Attivo dopo 20 pip di profitto con passo di 5 pip."
            }
        )
        db.add(default_strat)
        db.commit()
        strategies = [default_strat]
        
    return strategies


@app.post("/api/strategies", response_model=StrategyResponse, status_code=status.HTTP_201_CREATED)
def create_strategy(strategy_data: StrategyCreate, db: Session = Depends(get_db)):
    """Crea una nuova strategia nel Database con controllo duplicati sul titolo."""
    # Controllo ID
    existing_id = db.query(DBStrategy).filter(DBStrategy.id == strategy_data.id).first()
    if existing_id:
        raise HTTPException(status_code=400, detail="ID Strategia già esistente.")
    
    # Controllo Titolo Duplicato
    existing_title = db.query(DBStrategy).filter(DBStrategy.title.like(strategy_data.title.strip())).first()
    if existing_title:
        raise HTTPException(status_code=400, detail=f"Esiste già una strategia con il nome '{strategy_data.title}'.")

    now = datetime.now()
    db_strategy = DBStrategy(
        id=strategy_data.id,
        title=strategy_data.title.strip(),
        created=now.isoformat(),
        createdDisplay=now.strftime("%d/%m/%Y, %H:%M:%S"),
        modified=now.strftime("%d/%m/%Y, %H:%M:%S"),
        isSavedDB=True,
        isSavedGit=True,
        code=strategy_data.code,
        blocks=strategy_data.blocks.dict()
    )
    
    db.add(db_strategy)
    db.commit()
    db.refresh(db_strategy)
    return db_strategy


@app.put("/api/strategies/{strategy_id}", response_model=StrategyResponse)
def update_strategy(strategy_id: str, updated_data: dict, db: Session = Depends(get_db)):
    """Aggiorna i blocchi di testo della strategia o il suo codice sorgente con controllo duplicati."""
    db_strategy = db.query(DBStrategy).filter(DBStrategy.id == strategy_id).first()
    if not db_strategy:
        raise HTTPException(status_code=404, detail="Strategia non trovata.")

    now = datetime.now()
    
    # CONTROLLO DUPLICATO NOME: Se viene inviato un nuovo titolo, verifica che non esista già
    if "title" in updated_data:
        new_title = updated_data["title"].strip()
        # Cerca se esiste un'ALTRA strategia (con ID diverso) che ha già questo titolo (case-insensitive)
        duplicate = db.query(DBStrategy).filter(
            DBStrategy.title.like(new_title), 
            DBStrategy.id != strategy_id
        ).first()
        
        if duplicate:
            raise HTTPException(
                status_code=400, 
                detail=f"Impossibile rinominare: esiste già una strategia chiamata '{new_title}'."
            )
        db_strategy.title = new_title

    if "code" in updated_data:
        db_strategy.code = updated_data["code"]
    if "blocks" in updated_data:
        db_strategy.blocks = updated_data["blocks"]
        
    db_strategy.modified = now.strftime("%d/%m/%Y, %H:%M:%S")

    db.commit()
    db.refresh(db_strategy)
    return db_strategy


@app.delete("/api/strategies/{strategy_id}")
def delete_strategy(strategy_id: str, db: Session = Depends(get_db)):
    """Elimina definitivamente la strategia cercata dal database locale."""
    db_strategy = db.query(DBStrategy).filter(DBStrategy.id == strategy_id).first()
    if not db_strategy:
        raise HTTPException(status_code=404, detail="Strategia inesistente.")
    
    db.delete(db_strategy)
    db.commit()
    return {"detail": "Strategia rimossa con successo dal database locale."}


if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="127.0.0.1", port=8000, reload=True)