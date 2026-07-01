import os
import json
import subprocess
from datetime import datetime
from pathlib import Path

# Percorso: andrea-omnihub/trading/strategie/
BASE_PATH = Path(__file__).resolve().parent.parent / "trading" / "strategie"

def sync_create_or_update(strategy):
    """Crea o aggiorna i file su GitHub."""
    safe_name = strategy.title.replace(" ", "_")
    folder = BASE_PATH / safe_name
    folder.mkdir(parents=True, exist_ok=True)
    
    # Scrittura Codice
    # Nota: Assumiamo .cs per ora, potresti aggiungere logica per estensione
    ext = ".cs" 
    with open(folder / f"codice{ext}", "w", encoding="utf-8") as f:
        f.write(strategy.code)
        
    # Scrittura Metadata
    metadata = {
        "nome_strategia": strategy.title,
        "ultima_modifica": strategy.modified,
        "fondamenta": strategy.blocks
    }
    with open(folder / "metadata.json", "w", encoding="utf-8") as f:
        json.dump(metadata, f, indent=4)
        
    # Git Push
    subprocess.run(["git", "add", str(folder)], check=True)
    subprocess.run(["git", "commit", "-m", f"feat(strategie): sync {strategy.title}"], check=True)
    subprocess.run(["git", "push"], check=True)

def sync_delete(strategy_title):
    """Rimuove la cartella da GitHub."""
    safe_name = strategy_title.replace(" ", "_")
    folder = BASE_PATH / safe_name
    if folder.exists():
        import shutil
        shutil.rmtree(folder)
        subprocess.run(["git", "add", "trading/strategie"], check=True)
        subprocess.run(["git", "commit", "-m", f"feat(strategie): delete {strategy_title}"], check=True)
        subprocess.run(["git", "push"], check=True)