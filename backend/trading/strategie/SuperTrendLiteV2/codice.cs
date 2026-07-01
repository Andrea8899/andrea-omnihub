using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using cAlgo.Indicators;


namespace BB_SuperTrendEA
{
    
    //------------------------------------ ENTRATA --------------------------------//
    // DIREZIONI DI ORDINE
    public enum direzioneOrdine{
        Sell_Buy,
        Sell,
        Buy,
        Nessuno
    }
    
    
    // TIPI DI STRATEGIE PER ENTRATA
    // RotturaSessioneAsiatica : entra a mercato se viene rotto o il max o il min della sessione asiatica
    // CambioColoreSuperTrend : entra a mercato se il supertrend cambia colore da rosso a verde o viceversa
    public enum modalitaEntrataOrdine
    {
        RotturaSessioneAsiatica,
        CambioColoreSuperTrend
    }
    
    // LIMITI PER L'ENTRATA
    public enum limitiEntrataOrdine
    {
        Nessuno
    }
    
    
    
    //------------------------------------ USCITA --------------------------------//
    // TIPI DI STRATEGIE PER USCITA
    // ColoreSuperTrendOpposto : esce dal mercato una volta che chiusa una nuova candela il supertrend mostra il colore opposto rispetto a quello del trade
    public enum modalitaUscitaOrdine
    {
        ColoreSuperTrendOpposto
    }
    
    // LIMITI PER L'USCITA
    public enum limitiUscitaOrdine
    {
        Nessuno        
    }
    
    
    //------------------------------------ STOP LOSS/TRAILING STOP --------------------------------//
    // TIPI DI STRATEGIE PER STOP LOSS
    // primoValoreSuperTrend : si mette lo stop loss solo sul livello del supertrend riferito alla prima candela del trade
    // massimoMinimoTrendPrecedente : si mette lo stop loss solo sul livello 
    //                                      - di prezzo massimo se il supertrend del trend precedente è rialzista e adesso si sta facendo un ordine sell
    //                                        il prezzo per esatezza sarebbe il massimo tra i massimi di candela dalla prima candela del supertrend rialzista e l'ultima candela dello steso supertrend
    //                                      - di prezzo minimo se il supertrend del trend precedente è ribassista e adesso si sta facendo un ordine buy
    //                                        il prezzo per esatezza sarebbe il minimo tra i minimi di candela dalla prima candela del supertrend ribassista e l'ultima candela dello steso supertrend
    // primoValoreOMinMaxTrendPrecedente : si mette lo stop loss prendendo il valore minimo tra il prezzo che esce da primoValoreSuperTrend e massimoMinimoTrendPrecedente                 
    public enum modalitaStopLoss{ 
        Nessuno,
        primoValoreOMinMaxTrendPrecedente,
        primoValoreSuperTrend,
        massimoMinimoTrendPrecedente
    }
    
    public enum misuraStopLoss{ 
        Nessuno,
        Stretto,
        Ampio
    }
    
    // Strategie di TrailingStop
    // LineaSuperTrend : si sposta lo stop loss ad ogni chiusura candela sul valore del supertrend della candela precedente
    public enum StrategieTrailingStop{ 
        Nessuno,
        LineaSuperTrend,
        TrailingSoloBreakEven,
        TrailingBreakEvenEPoi
    }
    
    
    //------------------------------------ BREAK-EVEN/PARZIALI --------------------------------//
    
    
    
    
    
    
    
    [Robot(TimeZone = TimeZones.CentralEuropeStandardTime, AccessRights = AccessRights.FullAccess)]// EEuropeStandardTime(+2) CentralEuropeStandardTime(+1)
    public class StrategiaSuperTrendLiteV2 : Robot
    {

        //------------FILTRI MESI ------------
        [Parameter("Gennaio Operatività", DefaultValue = true,Group = "Mese Filtro")]
        public bool gennaioOperatività { get; set; }
        [Parameter("Febbraio Operatività", DefaultValue = true,Group = "Mese Filtro")]
        public bool febbraioOperatività { get; set; }
        [Parameter("Marzo Operatività", DefaultValue = true,Group = "Mese Filtro")]
        public bool marzoOperatività { get; set; }
        [Parameter("Aprile Operatività", DefaultValue = true,Group = "Mese Filtro")]
        public bool aprileOperatività { get; set; }
        [Parameter("Maggio Operatività", DefaultValue = true,Group = "Mese Filtro")]
        public bool maggioOperatività { get; set; }
        [Parameter("Giugno Operatività", DefaultValue = true,Group = "Mese Filtro")]
        public bool giugnoOperatività { get; set; }
        [Parameter("Luglio Operatività", DefaultValue = true,Group = "Mese Filtro")]
        public bool luglioOperatività { get; set; }
        [Parameter("Agosto Operatività", DefaultValue = true,Group = "Mese Filtro")]
        public bool agostoOperatività { get; set; }
        [Parameter("Settembre Operatività", DefaultValue = true,Group = "Mese Filtro")]
        public bool settembreOperatività { get; set; }
        [Parameter("Ottobre Operatività", DefaultValue = true,Group = "Mese Filtro")]
        public bool ottobreOperatività { get; set; }
        [Parameter("Novembre Operatività", DefaultValue = true,Group = "Mese Filtro")]
        public bool novembreOperatività { get; set; }
        [Parameter("Dicembre Operatività", DefaultValue = true,Group = "Mese Filtro")]
        public bool dicembreOperatività { get; set; }
        
        
        //------------SUPERTREND------------
        [Parameter("Supertrend Period", DefaultValue = 13,Group = "SuperTrend")]
        public int SuperPeriod { get; set; }
        [Parameter("Supertrend Multiplier", DefaultValue = 1,Group = "SuperTrend")]
        public double SuperMulti { get; set; }
        [Parameter("Supertrend Source", DefaultValue = MovingAverageType.Exponential,Group = "SuperTrend")]
        public MovingAverageType SuperSource { get; set; }

        
        // PARAMETRI
        [Parameter("Rischio in % per ogni trade",DefaultValue = 1,Group ="MM")]
        public double riskPerc { get; set; }
        
        [Parameter("tipi direzioni di entrata",Group = "Entrata")]
        public direzioneOrdine dirOrdine { get; set; }
        [Parameter("modalità entrata",Group = "Entrata")]
        public modalitaEntrataOrdine modEntrataOrd { get; set; }  
        [Parameter("1 Ordine X GG X Direzione",Group = "Entrata")]
        public bool unOrdineXDirezionGG { get; set; }
        [Parameter("orario alta volatilità",Group = "Entrata")]
        public bool orarioAltaVol_Lmt_Entrata { get; set; }
        [Parameter("orarioSessioneAsiaticaEntrata",DefaultValue = 8,Group = "Entrata")]
        public int orarioSessioneAsiaticaEntrata { get; set; }
        [Parameter("limite entrata",Group = "Entrata")]
        public limitiEntrataOrdine limEntrataOrdine { get; set; }
        
        // giorni min e max es: min = martedì, max = giovedì 
        // allora i giorni in cui operare sono martedì - mercoledì - giovedì - venerdì
        
        [Parameter("Lunedì", DefaultValue = true, Group ="GG Settimana Operatività")]
        public bool isLunedi { get; set; }
        [Parameter("Martedì", DefaultValue = true, Group ="GG Settimana Operatività")]
        public bool isMartedi { get; set; }
        [Parameter("Mercoledì", DefaultValue = true, Group ="GG Settimana Operatività")]
        public bool isMercoledi { get; set; }
        [Parameter("Giovedì", DefaultValue = true, Group ="GG Settimana Operatività")]
        public bool isGiovedi { get; set; }
        [Parameter("Venerdì", DefaultValue = true, Group ="GG Settimana Operatività")]
        public bool isVenerdi { get; set; }
        
        [Parameter("modalità uscita",Group = "Uscita")]
        public modalitaUscitaOrdine modUscitaOrd { get; set; }
        [Parameter("Chiusura Ordine Fine Giornata",Group = "Uscita")]
        public bool chiusuraFineGiornata { get; set; }
        [Parameter("orario alta volatilità per uscire",Group = "Uscita")]
        public bool orarioAltaVol_Lmt_Uscita { get; set; } 
        [Parameter("Ora Chiusura Fine Giornata",DefaultValue = 22,Group = "Uscita")]
        public int oraChiusuraFineGiornata { get; set; }
        [Parameter("Minuti Chiusura  Fine Giornata",DefaultValue = 0,Group = "Uscita")]
        public int minutiChiusuraFineGiornata { get; set; }
        [Parameter("uscita max perdita perc",Group = "Uscita")]
        public bool isMaxPerditaPerc { get; set; }
        
        [Parameter("modalità stop loss",Group = "Stop Loss")]
        public modalitaStopLoss modStopLossOrd { get; set; }
        [Parameter("stop loss misura",Group = "Stop Loss")]
        public misuraStopLoss misuraSL { get; set; }
        
        //!!!! DA FARE IL MOLTIPLICATORE DELLA MISURA DI STOP LOSS ES: x1.1  oppure x1.5
        
        
        [Parameter("stop loss perc risk",Group = "Stop Loss")]
        public bool stopLossPercRisk { get; set; }
        [Parameter("Strategia Trailing Stop",Group = "Stop Loss")]
        public StrategieTrailingStop strategiaTrailingStop { get; set; }
        
      
        

        // Variabili
        cAlgo.SuperTrendExperto sp = new cAlgo.SuperTrendExperto();
        double initialBalance = 0.0;
        double initialEquity = 0.0;
        double maxSessioneAsiatica = 0.0;
        double minSessioneAsiatica = 0.0;
        public int startAsian = 0; 
        public int finishAsian = 8;
        public int startAsianRect = 1; 
        public int finishAsianRect = 10;
        DateTime startDateBacktesting;
        DateTime endDateBacktesting;
        
        // -------- I FILE CSV ----------- //
        CBotPerformanceDataExtractor Extractor;
        // usato per prelevare i parametri in input e i relativi output di un backtest senza salvare i singoli trade
        StringBuilder txtCsvBacktestFinale = new StringBuilder(); 
        
        // usato per prelevare i parametri di ogni singolo trade fatto in backtest
        StringBuilder txtCsvEachTradeBacktest = new StringBuilder();
        
        
        // -------- BACKTEST ----------- //
        string valuesAllStatisticsEachTrade = ""; // variabile dove viene aggiunti i dati statistici di un trade ogni qualvolta appunto si aggiunge un nuovo trade
        double winRate = 0.0;
        
        

        protected override void OnStart()

        {
            sp = Indicators.GetIndicator<cAlgo.SuperTrendExperto>(SuperPeriod, SuperMulti, SuperSource);
            sp.AddToChart();
            //Backtesting.Completed += Backtesting_Completo;
            
            // SEZIONE SCARICAMENTO DATI BACKTEST
            initialBalance = Account.Balance;
            initialEquity = Account.Equity;
            Extractor = new CBotPerformanceDataExtractor();
            startDateBacktesting = Server.Time;
            
            
        }

        protected override void OnTick()
        {
            closeOperationMaxPerditaTrade();
            fineGiornata();
        }


        protected override void OnBar()
        {
            modEntrataOrdine();
        }
        
        protected override void OnBarClosed()
        {            
            modUscitaOrdine();
            modTrailingStop();
        }
        
        protected override void OnStop(){
            endDateBacktesting = Server.Time;
            writeDataOutputBacktest();
            writeInputOnBacktestHistory();
        }
        
        
        
        //------------------------------------------------------------------------------------------------------------ //
        //                                                 FUNZIONI PER ORDINI                                         // 
        //-------------------------------------------------------------------------------------------------------------//
        
        //----------------------------------------------------------------------------------- //
        //                                    ENTRATA                                         // 
        //------------------------------------------------------------------------------------//
        //+++++++++ LIMITI ++++++++ //
        
        public bool limitiEntrata(){
            bool isLimite = false;
            if( limEntrataOrdine == limitiEntrataOrdine.Nessuno ){
                isLimite = false;
            }
            if (orarioAltaVol_Lmt_Entrata == true && isLimite == false){
                isLimite = orarioAltaVolatilità();
            }
            return isLimite;
        }
        
        //+++++++++ ENTRATA ++++++++ //
        public void rotturaSessioneAsiatica(){
            // per prima cosa devo controllare se sono dopo l'orario di fine sessione asiatica 
            // in secondo luogo devo controllare che il prezzo o sia sotto il minimo di sessione asiatica o sia sopra il massimo di sessione asiatica
            DateTime now = Server.Time;
            int hourStartAsianSession = startAsian;
            int hourFinishAsianSession = orarioSessioneAsiaticaEntrata;
            DateTime midnight = new DateTime(now.Year,now.Month,now.Day,0,0,0);// mezzanotte            
            DateTime startAsianSessionDate = midnight.AddHours(hourStartAsianSession);
            DateTime finishAsianSessionDate = midnight.AddHours(hourFinishAsianSession); 
            //Print("startAsianSessionDate = "+startAsianSessionDate+" finishAsianSessionDate = "+finishAsianSessionDate);
            maxSessioneAsiatica = GetHighestPriceDecr(startAsianSessionDate,finishAsianSessionDate);
            minSessioneAsiatica = GetLowestPriceDecr(startAsianSessionDate,finishAsianSessionDate);
            //Print("hourStartAsianSession = "+hourStartAsianSession+" hourFinishAsianSession = "+hourFinishAsianSession+" maxSessioneAsiatica = "+maxSessioneAsiatica+" minSessioneAsiatica = "+minSessioneAsiatica);
            double actualPrice = Symbol.Bid;
            int actualHour = now.Hour;
            string superTrendDirezioneAttualeCandela = getSuperTrendPosNeg(0);
            string superTrendDirezionePrimaCandela = getSuperTrendPosNeg(1);
            double lastValueSuperTrend =  0.0;
            // indico che valore è il supertren della candela precedente
            if (superTrendDirezionePrimaCandela == "superTrendRialzo")
            {            
                lastValueSuperTrend = getSuperTrendValueByIndex("superTrendRialzo",1);
            }else if (superTrendDirezionePrimaCandela == "superTrendRibasso")
            {            
                lastValueSuperTrend = getSuperTrendValueByIndex("superTrendRibasso",1);
            }
            //Print("lastValueSuperTrend = "+lastValueSuperTrend);
            if ( actualHour >= orarioSessioneAsiaticaEntrata && (actualHour < oraChiusuraFineGiornata)){
                
                double lotSize = getVolumeForMarket(Symbol);
                //------------------- BUY ----------------//
                ///controllo che il prezzo se deve andare buy deve essere più alto o della lowLine o della highLine della candela precedente
                if ( actualPrice >= maxSessioneAsiatica && superTrendDirezioneAttualeCandela == "superTrendRialzo" && actualPrice > lastValueSuperTrend){                    
                    //double sl =  getSuperTrendValueByIndex("superTrendRialzo",0);
                    double sl =  getSuperTrendValueByIndex("superTrendRialzo",1);
                    //Print("Stop Loss BUY = "+sl);
                    double lotti = getLottoStopLossPercRisk(Symbol.Bid,sl);
                    double lottiConCommissioni = getLotsWithCommissions(Symbol.Bid,sl);
                    //Print("Lotti = "+lotti+" lottiConCommissioni = "+lottiConCommissioni);
                    // la candela precedente deve essere rialzista
                    double closePrecedente = Bars.ClosePrices.Last(1);
                    double openPrecedente = Bars.OpenPrices.Last(1);
                    bool isLimitNOrdXDir = false;
                    if (unOrdineXDirezionGG){
                        if(nOrdiniXDirezioneXDeterminatoGiorno(Server.Time,"buy") > 0){
                            isLimitNOrdXDir = true;
                        }
                    }
                    if ( closePrecedente > openPrecedente && isLimitNOrdXDir == false ){
                        if ( lottiConCommissioni > 0 ){ // vuol dire che il calcolo del lottoConCommissioni è al di sopra della minima size
                            ExecuteMarketOrder(TradeType.Buy, SymbolName, lottiConCommissioni*lotSize, "StrategiaSuperTrendLiteV2",0,0); 
                            modStopLossOrdine("Buy",1,2);
                        }else{
                            Print("Ordine non eseguibile per size al di sotto della minima");
                        }
                    }
                }
                //------------------- SELL ----------------//
                else if ( actualPrice <= minSessioneAsiatica && superTrendDirezioneAttualeCandela == "superTrendRibasso" && actualPrice < lastValueSuperTrend){
                    //double sl =  getSuperTrendValueByIndex("superTrendRibasso",0);
                    double sl =  getSuperTrendValueByIndex("superTrendRibasso",1);
                    //Print("Stop Loss SELL = "+sl);
                    double lotti = getLottoStopLossPercRisk(Symbol.Bid,sl);
                    double lottiConCommissioni = getLotsWithCommissions(Symbol.Bid,sl);
                    //Print("Lotti = "+lotti+" lottiConCommissioni = "+lottiConCommissioni);
                    // la candela precedente deve essere ribassista
                    double closePrecedente = Bars.ClosePrices.Last(1);
                    double openPrecedente = Bars.OpenPrices.Last(1);
                    bool isLimitNOrdXDir = false;
                    if (unOrdineXDirezionGG){
                        if(nOrdiniXDirezioneXDeterminatoGiorno(Server.Time,"sell") > 0){
                            isLimitNOrdXDir = true;
                        }
                    }
                    if ( closePrecedente < openPrecedente && isLimitNOrdXDir == false){
                        if ( lottiConCommissioni > 0 ){ // vuol dire che il calcolo del lottoConCommissioni è al di sopra della minima size
                            ExecuteMarketOrder(TradeType.Sell, SymbolName, lottiConCommissioni*lotSize, "StrategiaSuperTrendLiteV2",0,0); 
                            modStopLossOrdine("Sell",1,2);
                        }else{
                            Print("Ordine non eseguibile per size al di sotto della minima");
                        }
                    }
                }
            }
        
        }
        
        
        
        //!!! PROGRAMMATA PER FUNZIONARE CON ONBARCLOSED()
        // questa funzione entra a mercato controllando per prima cosa il colore del supertrend riferito alla seconda candela precedente
        // e successivamente confrontarla con il colore della prima candela precedente
        // nel caso le direzioni del supertrend per le prime due candele precedenti sono contrari allora entro a mercato
        public void cambioColoreSuperTrend3(){  
            string superTrendDirezionePrimaCandela = getSuperTrendPosNeg(2);
            string superTrendDirezioneAttualeCandela = getSuperTrendPosNeg(1);  
            //Print("superTrendDirezionePrimaCandela = "+superTrendDirezionePrimaCandela+" --- superTrendDirezioneAttualeCandela = "+superTrendDirezioneAttualeCandela);
            //----- BUY ------- ( la 1° candela ha supertrend ribassista e la candela attuale ha supertrend rialzista)
            if (superTrendDirezionePrimaCandela == "superTrendRibasso" && superTrendDirezioneAttualeCandela == "superTrendRialzo" && dirOrdine != direzioneOrdine.Sell && dirOrdine != direzioneOrdine.Nessuno)
            {
                double lotSize = getVolumeForMarket(Symbol);
                //double sl =  getSuperTrendValueByIndex("superTrendRialzo",1);
                //double sl = massimoMinimoTrendPrecedente("buy",2);
                double sl = getStopLossForCalcoli("buy",1,2);
                //Print("BUY sl = "+sl);
                double lotti = getLottoStopLossPercRisk(Symbol.Bid,sl);
                double lottiConCommissioni = getLotsWithCommissions(Symbol.Bid,sl);
                //Print("lotti = "+ lotti);
                double lastValueTrendPrecedente = Math.Round(getSuperTrendValueByIndex("superTrendRibasso",2),Symbol.Digits);
                double lastClose = Bars.ClosePrices.Last(1);
                //Print("BUY "+ lastValueTrendPrecedente +" "+ lastClose);
                // controllo che il prezzo non sia al di sopra del proprio stop loss
                if ( lastValueTrendPrecedente <= lastClose){                    
                    if ( lottiConCommissioni > 0 ){ // vuol dire che il calcolo del lottoConCommissioni è al di sopra della minima size
                        //ExecuteMarketOrder(TradeType.Buy, SymbolName, lottiConCommissioni*lotSize, "StrategiaSuperTrendLiteV2",0,0); 
                        ExecuteMarketOrder(TradeType.Buy, SymbolName, lottiConCommissioni, "StrategiaSuperTrendLiteV2",0,0);
                        modStopLossOrdine("buy",1,2);
                    }else{
                        Print("Ordine non eseguibile per size al di sotto della minima");
                    }
                }             
            }
            //----- SELL ------- ( la 1° candela ha supertrend rialzista e la candela attuale ha supertrend ribassista)
            else if (superTrendDirezionePrimaCandela == "superTrendRialzo" && superTrendDirezioneAttualeCandela == "superTrendRibasso" && dirOrdine != direzioneOrdine.Buy && dirOrdine != direzioneOrdine.Nessuno)
            {
                double lotSize = getVolumeForMarket(Symbol);
                //double sl =  getSuperTrendValueByIndex("superTrendRibasso",1);
                //double sl = massimoMinimoTrendPrecedente("sell",2)
                double sl = getStopLossForCalcoli("sell",1,2);
                //Print("SELL sl = "+sl);
                double lotti = getLottoStopLossPercRisk(Symbol.Bid,sl);
                double lottiConCommissioni = getLotsWithCommissions(Symbol.Bid,sl);
                //Print("lotti = "+ lotti);
                double lastValueTrendPrecedente = Math.Round(getSuperTrendValueByIndex("superTrendRialzo",2),Symbol.Digits);
                double lastClose = Bars.ClosePrices.Last(1);
                //Print("SELL "+ lastValueTrendPrecedente +" "+ lastClose);
                if ( lastValueTrendPrecedente >= lastClose){ 
                    if ( lottiConCommissioni > 0 ){ // vuol dire che il calcolo del lottoConCommissioni è al di sopra della minima size
                        //ExecuteMarketOrder(TradeType.Sell, SymbolName, lottiConCommissioni*lotSize, "StrategiaSuperTrendLiteV2",0,0);
                        ExecuteMarketOrder(TradeType.Sell, SymbolName, lottiConCommissioni, "StrategiaSuperTrendLiteV2",0,0);
                        modStopLossOrdine("sell",1,2);
                    }else{
                        Print("Ordine non eseguibile per size al di sotto della minima");
                    }
                }            
                
            }
        }
        
        // funzione che gestisce la modalità con la quale si dovrebbe entrare a mercato
        public void modEntrataOrdine(){ 
            bool limit = limitiEntrata(); 
            int hourFinishGiornata = Symbol.MarketHours.TimeTillClose().Hours; // restituisce il numero di minuti dalla chiusura del mercato
            // controllo che non ci siano posizione aperte nel caso contrario non devo usare nessuna modalità di apertura degli ordini
            DayOfWeek giornoSettAttuale = Server.Time.DayOfWeek;
            if (Positions.Count == 0  && limit == false && hourFinishGiornata > 0 && Server.Time.Hour < oraChiusuraFineGiornata && isDayOfWeekXOperare(giornoSettAttuale) && isMonthForTrade() )
            {                                
                if (modEntrataOrd == modalitaEntrataOrdine.RotturaSessioneAsiatica)
                {
                    rotturaSessioneAsiatica();
                }
                if (modEntrataOrd == modalitaEntrataOrdine.CambioColoreSuperTrend)
                {
                    cambioColoreSuperTrend3();
                }
            } 
        }
        
        
        
        
        //----------------------------------------------------------------------------------- //
        //                                    USCITA                                          // 
        //------------------------------------------------------------------------------------// 
        //---------------------------------------------- //
        //                LIMITI USCITA                 // 
        //-----------------------------------------------//
        // questa funzione restituisce :
        //                      - true : se dopo aver controllato che il limite è impostato a true si esegue la determinata
        //                               funzione e ci si trova nel limite. Di conseguenza il programma NON ENTRA.
        //                      - false : avviene se nessun limite è stato impostato a true oppure se eseguendo le determinate
        //                                funzioni non ci si trova in nessun limite. Di conseguenza il programma ENTRA.
        // 1. OrarioAltaVolatilità : da evitare alcuni orari per operare dal momento che si presenta alta volatilità che porta 
        //                        o a non eseguire correttamente gli ordini oppure a non chiudere al giusto prezzo di stop loss
        
        //----------------------------------------------      1.    ----------------------------------------------//
        // questa funzione restituisce true se ci si trova in alcuni orari dove si trova alta volatilità. Al momento per le conosenze
        // che ho in questo momento, imposto manualmente gli orari più volatili. Nel futuro docrò usare strumenti come l'ATR per controllare
        // nel passato quali siano per quel determinato mercato gli orari più volatili. Questo limite è dovuto dal momento che durante
        // questi orari l'alta volatilità potrebbe portare a salti considerevoli del prezzo che ritardano sia l'entrata a mercato 
        // e sia non eseguono lo stop loss al prezzo già stabilito da me.
        
        public bool orarioAltaVolatilità(){
            bool orarioVolatile = false; // di defalut considero l'orario attuale non volatile
            DateTime now =  Server.Time;
            int hourActual = setGiustaOra_OrarioLegale_Solare(now,now.Hour);
            int minutesActual = now.Minute;
            List<Tuple<int,int,int,int>> orariAltaVol = setOrariAltaVolatilità();
            //Print("Count di orariAltaVolatilità = "+orariAltaVol.Count);
            for (int i = 0; i < orariAltaVol.Count; i++)
            {
                Tuple<int,int,int,int> orarioAltaVol = orariAltaVol.ElementAt(i);
                int hourAltaVol_Iniziale = setGiustaOra_OrarioLegale_Solare(now,orarioAltaVol.Item1);
                int minutesAltaVol_Iniziale = orarioAltaVol.Item2;
                int hourAltaVol_Finale = setGiustaOra_OrarioLegale_Solare(now,orarioAltaVol.Item3);
                int minutesAltaVol_Finale = orarioAltaVol.Item4;
                // TIMEFRAME >= H1 ( non ha mezz'ora)
                if ( TimeFrame >= TimeFrame.Hour && hourActual >= hourAltaVol_Iniziale && hourActual <= hourAltaVol_Finale){
                    orarioVolatile = true;
                }
                // TIMEFRAME < H1 ( ha mezz'ora)
                else if (TimeFrame < TimeFrame.Hour &&hourActual >= hourAltaVol_Iniziale && hourActual <= hourAltaVol_Finale  && minutesActual >= minutesAltaVol_Iniziale && minutesActual <= minutesAltaVol_Finale){
                    orarioVolatile = true;
                }
           }
           return orarioVolatile;
        }
        
        
        
        
        
        public bool fineGiornata(){
            bool fineGiornata = false; // di defalut considero lo spread dell'orario attuale non alto
            int hourActual = Server.Time.Hour;
            int minuteActual = Server.Time.Minute;
            if ( Positions.Count > 0 && hourActual >= oraChiusuraFineGiornata - 1 && minuteActual == 59 && chiusuraFineGiornata == true){
                    //Print("Fine Giornata");
                    //fineGiornata = true;
                    ClosePosition(Positions[0]);
            }            
            return fineGiornata;
        }
        
        //+++++++++ LIMITI ++++++++ //
        
        public bool limitiUscita(){
            bool isLimite = false;            
            if (orarioAltaVol_Lmt_Uscita){
                isLimite = orarioAltaVolatilità();
            }
            
            return isLimite;
        }
        
        // questa funzione controlla se la cifra del trade in corso ha superato il limite in € che corrisponde alla riskPerc del conto
        public void closeOperationMaxPerditaTrade(){
            if (Positions.Count > 0)
            {
                DateTime now = Server.Time;
                //Print("Positions count = "+Positions.Count+" ora = "+now);
                //prelevo la massima perdita in € corrispodente alla riskPerc del conto
                double maxPerdita = getCifraMaxPerdita();
                double risultatoTradeAttuale = Positions[0].NetProfit;                
                //Print("maxPerdita = "+maxPerdita+" risultatoTradeAttuale  = "+risultatoTradeAttuale);
                if ( Math.Abs(risultatoTradeAttuale) >= maxPerdita && isMaxPerditaPerc == true && risultatoTradeAttuale < 0){
                    writeDataEachTradeBacktest(Positions[0]);
                    ClosePosition(Positions[0]);  
                }
            }            
        }
        
        // questa funzione esce dal mercato una volta che chiusa una nuova candela il supertrend mostra il colore opposto rispetto a quello del trade
        // controllando per prima cosa che direzione della posizione si tratta
        // ed insieme vedo se dopo della chiusura attuale la prima precedente e l'attuale candela hanno il colore del supertrend uno opposto dell'altro
        public void coloreSuperTrendOpposto(){
            string superTrendDirezionePrimaCandela = getSuperTrendPosNeg(1);
            string superTrendDirezioneAttualeCandela = getSuperTrendPosNeg(0); 
            //Print("superTrendDirezionePrimaCandela = "+superTrendDirezionePrimaCandela+" --- superTrendDirezioneAttualeCandela = "+superTrendDirezioneAttualeCandela);
            //----------- CHIUSURA ORDINE BUY  ------------ //( la 1° candela ha supertrend rialzista e la candela attuale ha supertrend ribassista)
            if ( Positions[0].TradeType == TradeType.Buy && superTrendDirezionePrimaCandela == "superTrendRialzo" && superTrendDirezioneAttualeCandela == "superTrendRibasso"){
                writeDataEachTradeBacktest(Positions[0]);
                ClosePosition(Positions[0]);
            }
            //----------- CHIUSURA ORDINE SELL  ------------ //( la 1° candela ha supertrend ribassista e la candela attuale ha supertrend rialzista)
            else if ( Positions[0].TradeType == TradeType.Sell &&  superTrendDirezionePrimaCandela == "superTrendRibasso" && superTrendDirezioneAttualeCandela == "superTrendRialzo"){
                writeDataEachTradeBacktest(Positions[0]);
                ClosePosition(Positions[0]);
            }
        }
        
        // funzione che gestisce la modalità con la quale si dovrebbe uscire a mercato
        public void modUscitaOrdine(){              
            bool isLimitUscita = limitiUscita(); 
            // controllo che non ci siano posizione aperte nel caso contrario non devo usare nessuna modalità di apertura degli ordini
            if (Positions.Count > 0)
            {                                
                if (isLimitUscita == true)
                {
                    // prima di chiudere la posizione devo prendere i dati statisitici
                    writeDataEachTradeBacktest(Positions[0]);
                    ClosePosition(Positions[0]);
                }
                
                else if (modUscitaOrd == modalitaUscitaOrdine.ColoreSuperTrendOpposto)
                {
                    coloreSuperTrendOpposto();
                }
            } 
        }
        
        //----------------------------------------------------------------------------------- //
        //                                    STOP LOSS                                       // 
        //------------------------------------------------------------------------------------//
        // TIPI DI STRATEGIE PER STOP LOSS
        // primoValoreSuperTrend : si mette lo stop loss solo sul livello del supertrend riferito alla prima candela del trade
        // massimoMinimoTrendPrecedente : si mette lo stop loss solo sul livello 
        //                                      - di prezzo massimo se il supertrend del trend precedente è rialzista e adesso si sta facendo un ordine sell
        //                                        il prezzo per esatezza sarebbe il massimo tra i massimi di candela dalla prima candela del supertrend rialzista e l'ultima candela dello steso supertrend
        //                                      - di prezzo minimo se il supertrend del trend precedente è ribassista e adesso si sta facendo un ordine buy
        //                                        il prezzo per esatezza sarebbe il minimo tra i minimi di candela dalla prima candela del supertrend ribassista e l'ultima candela dello steso supertrend
        // primoValoreOMinMaxTrendPrecedente : si mette lo stop loss prendendo il valore minimo tra il prezzo che esce da primoValoreSuperTrend e massimoMinimoTrendPrecedente                 
        
        
        public double primoValoreSuperTrend(int index){
            double pricePrimoValSuperTrend = 0.0;
            string superTrendDirezionePrimaCandela = getSuperTrendPosNeg(index); 
            //---- ORDINE BUY ------ //
            if (superTrendDirezionePrimaCandela == "superTrendRialzo" && Positions[0].TradeType == TradeType.Buy)
            {
                pricePrimoValSuperTrend = getSuperTrendValueByIndex("superTrendRialzo",index);
                //Print("BUY superTrendDirezionePrimaCandela = "+superTrendDirezionePrimaCandela+" pricePrimoValSuperTrend = "+pricePrimoValSuperTrend);
            }
            //---- ORDINE SELL ------ //
            else if (superTrendDirezionePrimaCandela == "superTrendRibasso" && Positions[0].TradeType == TradeType.Sell)
            {
                pricePrimoValSuperTrend = getSuperTrendValueByIndex("superTrendRibasso",index);
                //Print("SELL superTrendDirezionePrimaCandela = "+superTrendDirezionePrimaCandela+" pricePrimoValSuperTrend = "+pricePrimoValSuperTrend);
            }
            return pricePrimoValSuperTrend;
        }
        
        
        // questa funzione restituisce il massimo o il minimo all'interno del supertrend precedente all'ordine attuale
        // !!! al momento ho implementato solo il massimo della candela più in alto in un supertrend rialzista e 
        // il minimo della candela più in basso in un supertrend ribassista e non ancora implementato la chiusura della candela più in alto o quella più in basso
        public double massimoMinimoTrendPrecedente(string direzione, int index){
            double maxMinTrendPrecedente = 0.0; 
            //--------------- BUY ------------------//
            if (direzione == "buy")
            {                
                //Print("LowestLow = "+getMaxMinLevelOrCloseSuperTrend("LowestLow",index));
                maxMinTrendPrecedente = getMaxMinLevelOrCloseSuperTrend("LowestLow",index);
            }
            //--------------- SELL ------------------//
            else if (direzione == "sell")
            {                
                //Print("HighestHigh = "+getMaxMinLevelOrCloseSuperTrend("HighestHigh",index));
                maxMinTrendPrecedente = getMaxMinLevelOrCloseSuperTrend("HighestHigh",index);
            }
            return maxMinTrendPrecedente;
        }
        
        
        //questa funzione restituisce lo stop loss o più ampio o più stretto tra il risultato di primoValoreSuperTrend e il risultato di massimoMinimoTrendPrecedente
        public double primoValoreOMaxMinTrendPrecedente(string direzione, double primoValoreSuperTrend, double maxMinTrendPrecedente){
            double minSL = 0.0;
            //--------------- BUY ------------------// 
            if (direzione == "buy")
            {       
                //---- AMPIO ----//(lo stop loss deve avere un livello di prezzo più basso)
                if (misuraSL == misuraStopLoss.Ampio)
                {
                    minSL = Math.Min(primoValoreSuperTrend,maxMinTrendPrecedente);
                }
                //---- STRETTO ----//(lo stop loss deve avere un livello di prezzo più alto)
                else if (misuraSL == misuraStopLoss.Stretto)
                {
                    //Print(" STRETTO primoValoreSuperTrend = "+primoValoreSuperTrend+" maxMinTrendPrecedente = "+maxMinTrendPrecedente);
                    minSL = Math.Max(primoValoreSuperTrend,maxMinTrendPrecedente);
                }
            }
            //--------------- SELL ------------------//
            else if (direzione == "sell")
            {  
                //---- AMPIO ----//(lo stop loss deve avere un livello di prezzo più alto)
                if (misuraSL == misuraStopLoss.Ampio)
                {
                    minSL = Math.Max(primoValoreSuperTrend,maxMinTrendPrecedente);
                }
                //---- STRETTO ----//(lo stop loss deve avere un livello di prezzo più basso)
                else if (misuraSL == misuraStopLoss.Stretto)
                {
                    minSL = Math.Min(primoValoreSuperTrend,maxMinTrendPrecedente);
                }
            }
            return minSL;
        }
        
        
        
        // questa funzione viene chiamata quando voglio mettere lo stop loss
        public void modStopLossOrdine(string tipoOrdine,int indexPrimoValoreSuperTrend,int indexMassimoMinimoTrendPrecedente){
            // controllo quale strategia utilizzare per lo stop loss
            if (modStopLossOrd == modalitaStopLoss.primoValoreSuperTrend)
            {
                
                double stopLoss = primoValoreSuperTrend(indexPrimoValoreSuperTrend);
                Positions[0].ModifyStopLossPrice(stopLoss);
            }
            else if (modStopLossOrd == modalitaStopLoss.massimoMinimoTrendPrecedente)
            {
                double stopLoss = massimoMinimoTrendPrecedente(tipoOrdine,indexMassimoMinimoTrendPrecedente);
                //Print("Stop loss messo = "+stopLoss);
                Positions[0].ModifyStopLossPrice(stopLoss);
            }
            else if (modStopLossOrd == modalitaStopLoss.primoValoreOMinMaxTrendPrecedente)
            {
                double sl_primoValSuperTrend = primoValoreSuperTrend(indexPrimoValoreSuperTrend);
                double sl_maxMinTrendPrecedente = massimoMinimoTrendPrecedente(tipoOrdine,indexMassimoMinimoTrendPrecedente);
                double minSL = primoValoreOMaxMinTrendPrecedente(tipoOrdine,sl_primoValSuperTrend,sl_maxMinTrendPrecedente);
                Positions[0].ModifyStopLossPrice(minSL);
            }
            
        }
        
        
        // questa funzione restituisce lo stop loss per fare il calcolo del lottaggio ma non setta lo stop loss
        public double getStopLossForCalcoli(string tipoOrdine,int indexPrimoValoreSuperTrend,int indexMassimoMinimoTrendPrecedente){
            double slForCal = 0.0;
            if (modStopLossOrd == modalitaStopLoss.primoValoreSuperTrend)
            {
                if(tipoOrdine == "buy"){  
                    slForCal = getSuperTrendValueByIndex("superTrendRialzo",1);
                }else if(tipoOrdine == "sell"){  
                    slForCal = getSuperTrendValueByIndex("superTrendRibasso",1);
                }
            }
            else if (modStopLossOrd == modalitaStopLoss.massimoMinimoTrendPrecedente)
            {
                slForCal = massimoMinimoTrendPrecedente(tipoOrdine,indexMassimoMinimoTrendPrecedente);
            }
            else if (modStopLossOrd == modalitaStopLoss.primoValoreOMinMaxTrendPrecedente)
            {
                double sl_primoValSuperTrend = 0.0;
                double sl_maxMinTrendPrecedente = 0.0;
                if(tipoOrdine == "buy"){  
                    sl_primoValSuperTrend = getSuperTrendValueByIndex("superTrendRialzo",1);
                }else if(tipoOrdine == "sell"){  
                    sl_primoValSuperTrend = getSuperTrendValueByIndex("superTrendRibasso",1);
                }
                sl_maxMinTrendPrecedente = massimoMinimoTrendPrecedente(tipoOrdine,indexMassimoMinimoTrendPrecedente);
                slForCal = primoValoreOMaxMinTrendPrecedente(tipoOrdine,sl_primoValSuperTrend,sl_maxMinTrendPrecedente);
            }
            return slForCal;
        }
        
        
        // questa funzione sposta lo stop loss prendendo il valore del supertrend della candela precedente
        public void setTrailingStopLineaSuperTrend(){
            //--------- CAMBIO STOP LOSS BUY---------------//
            if ( Positions[0].TradeType == TradeType.Buy && Positions[0].NetProfit >= 0 ){
                double actualValueSuperTrend = getSuperTrendValueByIndex("superTrendRialzo",0);
                double actualPriceSL = (double) Positions[0].StopLoss;
                //Print("actualPriceSL = "+actualPriceSL);
                if ( actualValueSuperTrend > actualPriceSL ){                    
                    Positions[0].ModifyStopLossPrice(actualValueSuperTrend);
                }
            }
            //--------- CAMBIO STOP LOSS SELL---------------//
            else if ( Positions[0].TradeType == TradeType.Sell && Positions[0].NetProfit >= 0 ){
                double actualValueSuperTrend = getSuperTrendValueByIndex("superTrendRibasso",0);
                double actualPriceSL = (double) Positions[0].StopLoss;
                //Print("actualPriceSL = "+actualPriceSL);
                if ( actualValueSuperTrend < actualPriceSL ){
                    Positions[0].ModifyStopLossPrice(actualValueSuperTrend);
                }
                
            }
        }
        
        
        public void modTrailingStop(){ 
            if (Positions.Count > 0 && strategiaTrailingStop == StrategieTrailingStop.LineaSuperTrend)
            {
                setTrailingStopLineaSuperTrend();
            }
        }

                                    

        // questo metodo mi restituisce se il supertrend è a rialzo ovvero la linea inferiore(lowline) è diversa da Nan(nulla) o 
        // a ribasso ovvero la linea superiore(highline) è diversa da Nan(nulla)
        protected string getSuperTrendPosNeg(int index)
        {
            string superTrendDirezione = "Nessuna";
            int currentIndex = Bars.Count - 1 - index;
            double lastLowLineSuperTr = sp.LowLine[currentIndex];
            double lastHighLineSuperTr = sp.HighLine[currentIndex];
            
            // 1. il supertrend deve essere positivo cioè verde cioè ci deve essere una lowline con valore diverso da Nan
            if (!double.IsNaN(lastLowLineSuperTr))
            {
                superTrendDirezione = "superTrendRialzo";

            }else if (!double.IsNaN(lastHighLineSuperTr))
            {
                superTrendDirezione = "superTrendRibasso";

            }
            return superTrendDirezione;
        }
        
        // questo metodo mi restituisce il valore del supertrend per quel determinato indice di candela e a secondo se voglio 
        // per il supertrend a rialzo(lowline) o a ribasso(highline)
        protected double getSuperTrendValueByIndex(string direzione, int index)
        {
            double valueSuperTrend = 0.0;
            string superTrendDirezione = "Nessuna";
            int currentIndex = Bars.Count - 1 - index;
            double lastLowLineSuperTr = sp.LowLine[currentIndex];
            double lastHighLineSuperTr = sp.HighLine[currentIndex];
            
            // 1. il supertrend deve essere positivo cioè verde cioè ci deve essere una lowline con valore diverso da Nan
            if (!double.IsNaN(lastLowLineSuperTr) && direzione == "superTrendRialzo")
            {
                valueSuperTrend = lastLowLineSuperTr;

            }else if (!double.IsNaN(lastHighLineSuperTr) && direzione == "superTrendRibasso")
            {
                valueSuperTrend = lastHighLineSuperTr;

            }
            return valueSuperTrend;
        }

        
        //questa funzione restituisce o :
        //                      - il massimo livello di prezzo all'interno di un supertrend rialzista(highest(high))
        //                      - il minimo livello di prezzo all'interno di un supertrend ribassista(lowest(low))
        //                      - la massima chiusura di una candela all'interno di un supertrend rialzista(highest(close))
        //                      - la minima chiusura di una candela all'interno di un supertrend ribassista(lowest(low))
        // dove per typeLevel si intende appunto HighestHigh,LowestLow,HighestClose,LowestClose
        // mentre per startIndex si intende l'indice della candela da dove partire ad analizzare
        public double getMaxMinLevelOrCloseSuperTrend(string typeLevel,int startIndex){
            double lvlPrice = 0.0;            
            double finishIndex = Bars.Count - startIndex;
            for (int i = startIndex; i < finishIndex; i++)
            {
                
                int indexActual = Bars.Count - i ;
                double highActual = Bars.HighPrices[indexActual];
                double lowActual = Bars.LowPrices[indexActual];
                double closeActual = Bars.ClosePrices[indexActual];
                //---------- PRECEDENTE: SUPERTREND RIALZISTA; ATTUALE: SUPERTREND RIBASSISTA ----------//
                // HIGHESTHIGH  
                if (typeLevel == "HighestHigh" && getSuperTrendPosNeg(i) == "superTrendRialzo"){
                    // primo valore
                    if ( lvlPrice == 0 ){
                        lvlPrice = highActual;
                    }                    
                    // gli altri valori
                    else{
                        if ( highActual > lvlPrice){
                            lvlPrice = highActual;
                        }
                    }
                }
                // HIGHESTCLOSE
                else if (typeLevel == "HighestClose" && getSuperTrendPosNeg(i) == "superTrendRialzo"){
                    // primo valore
                    if ( lvlPrice == 0 ){
                        lvlPrice = closeActual;
                    }                    
                    // gli altri valori
                    else{
                        if ( closeActual > lvlPrice){
                            lvlPrice = closeActual;
                        }    
                    }
                }
                //---------- PRECEDENTE: SUPERTREND RIBASSISTA; ATTUALE: SUPERTREND RIALZISTA   ----------//
                // LOWESTLOW  
                else if (typeLevel == "LowestLow" && getSuperTrendPosNeg(i) == "superTrendRibasso"){
                    // primo valore
                    if ( lvlPrice == 0 ){
                        lvlPrice = lowActual;
                    }                    
                    // gli altri valori
                    else{
                        if ( lowActual < lvlPrice){
                            lvlPrice = lowActual;
                        }
                    }
                }
                // LOWESTCLOSE
                else if (typeLevel == "LowestClose" && getSuperTrendPosNeg(i) == "superTrendRibasso"){
                    // primo valore
                    if ( lvlPrice == 0 ){
                        lvlPrice = closeActual;
                    }                    
                    // gli altri valori
                    else{
                        if ( closeActual < lvlPrice){
                            lvlPrice = closeActual;
                        }
                    }
                }
                // CONDIZIONI DI USCITA DELLA RICERCA 
                if(lvlPrice > 0){
                    // buy
                    if((typeLevel == "HighestHigh"||typeLevel == "HighestClose") && getSuperTrendPosNeg(i) == "superTrendRibasso"){
                        return lvlPrice;
                    }
                    // sell
                    if((typeLevel == "LowestLow" || typeLevel == "LowestClose") && getSuperTrendPosNeg(i) == "superTrendRialzo"){
                        return lvlPrice;
                    }
                }
                
            }
            return lvlPrice;
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        //questa funzione restiuisce se du edate sono nello stesso giorno
        public bool isSameDay(DateTime date1, DateTime date2){
            bool sameDay = false;
            if ( date1.Year == date2.Year  &&  date1.Month == date2.Month  &&  date1.Day == date2.Day ){
                sameDay = true;
            }
            return sameDay;
        }
        
        //questa funzione restituisce il numero di ordini che se sono già stati aperti ordini per una determinata direzione in un determinato giorno 
        public int nOrdiniXDirezioneXDeterminatoGiorno(DateTime date, string direzione){
            int nOrdiniXDirOggi = 0;
            DateTime now = Server.Time;
            foreach (var trade in History)
            {
                TradeType directionTradeActual = trade.TradeType;
                DateTime dayTradeActual = trade.EntryTime;
                // ORDINE BUY
                if ( direzione == "buy" && directionTradeActual == TradeType.Buy && isSameDay(dayTradeActual,date) ){
                    nOrdiniXDirOggi += 1;
                }
                // ORDINE SELL
                else if ( direzione == "sell" && directionTradeActual == TradeType.Sell && isSameDay(dayTradeActual,date)){
                    nOrdiniXDirOggi += 1;
                }
            }
            //Print("nOrdiniXDirOggi = "+nOrdiniXDirOggi);
            return nOrdiniXDirOggi;
        }
        
        
        
        //------------------------------------------------------------------------------------------------------------ //
        //                                                 FUNZIONI PER CALCOLI                                        // 
        //-------------------------------------------------------------------------------------------------------------//        
        
        
        // questa funzione restituisce il numero di lotti da utilizzare partendo da una percentuale di rischio e uno stop loss
        public double getLottoStopLossPercRisk(double priceEntry,double priceSL){
            double lotto = 0.01;
            if (stopLossPercRisk == true){
                double cifra_rischio = getCifraMaxPerdita();
                double lotSize = getVolumeForMarket(Symbol);
                double pipValueLotto = Math.Round(Symbol.PipValue*lotSize,3);
                double stopLossPip = getPipTwoPrices(Math.Round(priceSL,Symbol.Digits),Math.Round(priceEntry,Symbol.Digits)); 
                if (lotSize == 1){ // indici 
                    lotto = Math.Ceiling(cifra_rischio/ (pipValueLotto*stopLossPip));
                }else if (lotSize == 100000) { //forex
                    lotto = Math.Round(cifra_rischio/ (pipValueLotto*stopLossPip),2);
                }
                else if (lotSize == 1000) { //forex
                    lotto = Math.Round(cifra_rischio/ (pipValueLotto*stopLossPip),2);
                }
                //Print("cifra_rischio = "+cifra_rischio+" pipValueLotto = "+pipValueLotto+" stopLossPip = "+stopLossPip+" lotto = "+lotto );
            }
            return lotto;
        }
        
        // questa funzione restituisce il numero di pip tra due livelli di prezzo
        private double getPipTwoPrices(double price1, double price2){
            double pipTwoPrices = 0;
            //Print("Symbol digits = "+Symbol.Digits+" Name = "+Symbol.Name);
            // Commodities WTI Petrolio
            if ( Symbol.Digits == 2  && Symbol.Name == "WTI"){
                pipTwoPrices = Math.Round( (Math.Abs(price1 - price2)  * Math.Pow(10,Symbol.Digits) ),2);
            }
            else if (Symbol.Name == "XAUUSD"){
                pipTwoPrices = Math.Round( (Math.Abs(price1 - price2)  * Math.Pow(10,Symbol.Digits) ),2);
            }else{
                pipTwoPrices = Math.Round( (Math.Abs(price1 - price2)  * Math.Pow(10,Symbol.Digits-1) ),2);
            }
            return pipTwoPrices;
        }
        
        
        
        // questo metodo restituisce la cifra che corrisponde la percentuale scelta massima che voglio perdere per ogni trade
        // devo calcolare però  ogni volta la cifra del trade in corso
        public double getCifraMaxPerdita(){
            double equity = Account.Equity;
            return (equity*riskPerc)/100;
        }
        
        
        // questa funzione restituisce il volume per gli ordini espressi o in unità o in lotti
        // su FP Markets :
        //          - le valute : in unità quindi per fare un lotto devo mettere 1* 100.000 dove 100.000 sarebbero le unità per un lotto
        //          - gli indici : in lotto quindi per fare un lotto devo mettere solo 1
        public int getVolumeForMarket(Symbol symbol){
            int volume = 1;
            int lotSizeAssetClass = (int)symbol.LotSize;
            // valute
            if (lotSizeAssetClass > 0){
                volume = lotSizeAssetClass;
            }
            //Print("lotSizeAssetClass = "+lotSizeAssetClass);
            return volume;
        }
        
        public int countDigit(double number) { 
          return (int)Math.Abs(Math.Log10(number)); 
        }
        
        //questa funzione restituisce quanto è il minimo volume da tradare e anche di quanti digits devo arrotondare il numero di lotti da usare
        public int getNDigitsForTradeLots(){
            int nDigitsXLots = 1;
            double volMin = Symbol.VolumeInUnitsMin;
            if ( volMin > 0 ){
                nDigitsXLots = countDigit(volMin);
            }
            //Print("Min Volume = "+volMin+" nDigitsXLots = "+nDigitsXLots);
            return nDigitsXLots;
        }
        
        //questa funzione restituisce il numero di lotti prendendo in considerazione anche le possibili commissioni
        public double getLotsWithCommissions(double priceEntry, double priceSL){
            double lots = 0.0;
            double lotsWithCommmissions = 0;
            try{
                if (stopLossPercRisk == true){
                    double cifra_rischio = getCifraMaxPerdita();
                    double lotSize = getVolumeForMarket(Symbol);
                    double pipValueLotto = Math.Round(Symbol.PipValue*lotSize,3);
                    double commissionsLotto = Symbol.Commission*2;// commissioni per un lotto
                    double stopLossPip = getPipTwoPrices(Math.Round(priceSL,Symbol.Digits),Math.Round(priceEntry,Symbol.Digits));
                    //Print("stopLossPip = "+stopLossPip);
                    int nDigitsMinVolume = getNDigitsForTradeLots();
                    lots = Math.Round(cifra_rischio/ (pipValueLotto*stopLossPip),nDigitsMinVolume);
                    //lotsWithCommmissions = Math.Round(cifra_rischio/ ((pipValueLotto*stopLossPip)+(lots*commissionsLotto)),nDigitsMinVolume);
                    lotsWithCommmissions = Symbol.VolumeForProportionalRisk(ProportionalAmountType.Balance,riskPerc,stopLossPip);
                    //Print("lotsWithCommmissions = "+lotsWithCommmissions);
                }
            }catch(Exception e){
                Print("Size calcolata troppo piccola");
            }
            
            return lotsWithCommmissions;
        
        }
        
        //...................................................... FUNZIONI TEMPORALI .............................................//
        
        //questa funzione controlla se il giorno corrente rientra tra i giorni della settimana per operare
        public bool isDayOfWeekXOperare(DayOfWeek dayActual){
            bool isDayOfWeek = false;
            List<DayOfWeek> daysOfWeek = new List<DayOfWeek>{DayOfWeek.Monday,DayOfWeek.Tuesday,DayOfWeek.Wednesday,DayOfWeek.Thursday,DayOfWeek.Friday};
            List<bool> daysOfWeekOp = new List<bool>{isLunedi,isMartedi,isMercoledi,isGiovedi,isVenerdi};
            for (int i = 0; i < daysOfWeek.Count; i++)
            {               
                if(dayActual == daysOfWeek.ElementAt(i) && daysOfWeekOp.ElementAt(i) == true){ 
                    isDayOfWeek = true;
                }
            }
            return isDayOfWeek;
        }
        
        
        
        // questa funzione restituisce l'ora modificata prendendo come input una data(orario) per capire se ci si trova nell'ora legale o solare
        public int setGiustaOra_OrarioLegale_Solare(DateTime orario){
            // una data può essere :
            //      1. prima sia startDateOraLegale e startDateOraSolare (gennaio-febbraio-marzo) ----> ORA SOLARE
            //      2. dopo startDateOraLegale e primo o uguale startDateOraSolare (aprile-maggio-giugno-luglio-agosto-settembre-ottobre) ----> ORA LEGALE
            //      3. dopo startDateOraLegale e dopo startDateOraSolare (novembre - dicembre) ----> ORA SOLARE
            int hourAggiustata = orario.Hour;
            // ORA LEGALE ( dall'ultima domenica di marzo e termina l'ultima domenica di ottobre)
            int giornoUltimaDomenicaMarzo = primoUltimoGiornoSettimanaByDate(new DateTime(orario.Year,3,1),"ultimo",DayOfWeek.Sunday);
            DateTime startDateOraLegale = new DateTime(orario.Year,3,giornoUltimaDomenicaMarzo);
            
            // ORA SOLARE ( dall'ultima domenica di ottobre e termina l'ultima domenica di marzo)
            int giornoUltimaDomenicaOttobre = primoUltimoGiornoSettimanaByDate(new DateTime(orario.Year,10,1),"ultimo",DayOfWeek.Sunday);
            DateTime startDateOraSolare = new DateTime(orario.Year,10,giornoUltimaDomenicaOttobre);
            //Print("orario.CompareTo(startDateOraLegale) = "+orario.CompareTo(startDateOraLegale)+" orario.CompareTo(startDateOraSolare) = "+orario.CompareTo(startDateOraSolare));
            // GENNAIO - FEBBRAIO - MARZO
            if(orario.CompareTo(startDateOraLegale) <= 0 && orario.CompareTo(startDateOraSolare) < 0){ //----> ORA SOLARE
                //Print("Ora Solare (gennaio-febbraio-marzo)");
                DateTime dateTimeRight = orario.AddHours(1);
                hourAggiustata = dateTimeRight.Hour;
                
            }
            // APRILE - MAGGIO - GIUGNO - LUGLIO - AGOSTO - SETTEMBRE - OTTOBRE
            else if(orario.CompareTo(startDateOraLegale) > 0 && orario.CompareTo(startDateOraSolare) < 0){ //----> ORA LEGALE
               // Print("Ora Legale (aprile-maggio-giugno-luglio-agosto-settembre-ottobre)");
            }
            // NOVEMBRE - DICEMBRE
            else if(orario.CompareTo(startDateOraLegale) > 0 && orario.CompareTo(startDateOraSolare) > 0){ //----> ORA SOLARE
                //Print("Ora Solare (novembre - dicembre)");
                DateTime dateTimeRight = orario.AddHours(1);
                hourAggiustata = dateTimeRight.Hour;
            }
            
            
            return hourAggiustata;
        }
        
        // questa funzione restituisce l'ora modificata prendendo come input l'ora da modificare(hour) e una data(now) per capire se ci si trova nell'ora legale o solare
        public int setGiustaOra_OrarioLegale_Solare(DateTime now,int hour){
            // una data può essere :
            //      1. prima sia startDateOraLegale e startDateOraSolare (gennaio-febbraio-marzo) ----> ORA SOLARE
            //      2. dopo startDateOraLegale e primo o uguale startDateOraSolare (aprile-maggio-giugno-luglio-agosto-settembre-ottobre) ----> ORA LEGALE
            //      3. dopo startDateOraLegale e dopo startDateOraSolare (novembre - dicembre) ----> ORA SOLARE
            int hourAggiustata = hour;
            // ORA LEGALE ( dall'ultima domenica di marzo e termina l'ultima domenica di ottobre)
            int giornoUltimaDomenicaMarzo = primoUltimoGiornoSettimanaByDate(new DateTime(now.Year,3,1),"ultimo",DayOfWeek.Sunday);
            DateTime startDateOraLegale = new DateTime(now.Year,3,giornoUltimaDomenicaMarzo);
            
            // ORA SOLARE ( dall'ultima domenica di ottobre e termina l'ultima domenica di marzo)
            int giornoUltimaDomenicaOttobre = primoUltimoGiornoSettimanaByDate(new DateTime(now.Year,10,1),"ultimo",DayOfWeek.Sunday);
            DateTime startDateOraSolare = new DateTime(now.Year,10,giornoUltimaDomenicaOttobre);
            //Print("orario.CompareTo(startDateOraLegale) = "+orario.CompareTo(startDateOraLegale)+" orario.CompareTo(startDateOraSolare) = "+orario.CompareTo(startDateOraSolare));
            // GENNAIO - FEBBRAIO - MARZO
            if(now.CompareTo(startDateOraLegale) <= 0 && now.CompareTo(startDateOraSolare) < 0){ //----> ORA SOLARE
                //Print("Ora Solare (gennaio-febbraio-marzo)");
                DateTime dateTimeRight = new DateTime(now.Year,now.Month,now.Day,hour,0,0).AddHours(1);
                hourAggiustata = dateTimeRight.Hour;
                
            }
            // APRILE - MAGGIO - GIUGNO - LUGLIO - AGOSTO - SETTEMBRE - OTTOBRE
            else if(now.CompareTo(startDateOraLegale) > 0 && now.CompareTo(startDateOraSolare) < 0){ //----> ORA LEGALE
               // Print("Ora Legale (aprile-maggio-giugno-luglio-agosto-settembre-ottobre)");
            }
            // NOVEMBRE - DICEMBRE
            else if(now.CompareTo(startDateOraLegale) > 0 && now.CompareTo(startDateOraSolare) > 0){ //----> ORA SOLARE
                //Print("Ora Solare (novembre - dicembre)");
                DateTime dateTimeRight = new DateTime(now.Year,now.Month,now.Day,hour,0,0).AddHours(1);
                hourAggiustata = dateTimeRight.Hour;
            }
            
            
            return hourAggiustata;
        }
        
        //questa funzione restituisce il giorno corrispodente all'ultima domenica del mese di date
        public int primoUltimoGiornoSettimanaByDate(DateTime date,string primo_ultimo,DayOfWeek giornoSettimana){
            int giornoUltimaDom = 0;
            DateTime meseSuccessivo = date.AddMonths(1);
            DateTime firstDayMeseSuccessivo = new DateTime(meseSuccessivo.Year,meseSuccessivo.Month,1);
            DateTime meseCorrente = firstDayMeseSuccessivo.AddDays(-1);
            int lastDayOfMonth = meseCorrente.Day;
            for (int i = 0; i < lastDayOfMonth; i++)
            {
                int dayCurrent = i+1;
                if (primo_ultimo == "ultimo"){
                    dayCurrent = lastDayOfMonth - i;
                }
                DateTime currentDate = new DateTime(meseCorrente.Year,meseCorrente.Month,dayCurrent);
                if(currentDate.DayOfWeek == giornoSettimana && giornoUltimaDom == 0){
                    //Print("i = "+i+" dayCurrent = "+dayCurrent+" currentDate = "+currentDate);
                    giornoUltimaDom = currentDate.Day;
                }
            }
            return giornoUltimaDom;
        }
        
        
        

        // con questo metodo controllo in quale giorno operare
        protected Boolean isDayOfWeekForTrade(DayOfWeek minDayWeek, DayOfWeek maxDayWeek)
        {
            Boolean isDayOfWeek = false;
            DayOfWeek dayOfWeekToday = Server.Time.DayOfWeek;
            int m = Server.Time.Month;
            //Print("Gionro della settimana di oggi = " + dayOfWeekToday + " minDayWeek = " + minDayWeek + " maxDayWeek = " + maxDayWeek);
            // 1. per prima cosa controllo che la data minima sia minore della maggiore
            if (minDayWeek > maxDayWeek)
            {
                //Print("Il minDayWeek è minore di maxDayWeek");
                return isDayOfWeek;
            }
            // 2. controllo che minDayWeek e maxDayWeek non siano di sabato e domenica
            if ( minDayWeek == DayOfWeek.Saturday || minDayWeek == DayOfWeek.Sunday || maxDayWeek == DayOfWeek.Saturday || maxDayWeek == DayOfWeek.Sunday){
                //Print("Parametri Sbagliati");
                return isDayOfWeek;
            }
            // 3. controllo semplicemente che il giorno in cui si opera rientri nel range
            if ( dayOfWeekToday >= minDayWeek  && dayOfWeekToday <= maxDayWeek){
                isDayOfWeek = true;
            }
            return isDayOfWeek;
        }

    
    
        public Boolean isMonthForTrade(){
            Boolean isMonth = false;
            int monthActual = Server.Time.Month;
            List<bool> mesiOperatività = new List<bool>(){gennaioOperatività,febbraioOperatività,marzoOperatività,aprileOperatività,maggioOperatività,giugnoOperatività,luglioOperatività,agostoOperatività,settembreOperatività,ottobreOperatività,novembreOperatività,dicembreOperatività};
            for (int i = 1; i < 13; i++)
            {
                if ( monthActual == i && mesiOperatività.ElementAt(i-1) == true)
                {
                    isMonth = true;
                }
            }
            
            return isMonth;
        }
        




        // con questo metodo ricreo l'azione del trailing stop ma scegliendo su quale strategia:
        //                  1. mettere lo stop loss a break-even quando il prezzo ha raggiunto la stessa misura in pip dello stop loss in takeprofit 
        //                  2. la stessa strategia del 1. ma con l'aggiunta di andare oltre al prezzo di entrata (da capire di quanto spostarlo)
        protected void setTrailingStop(StrategieTrailingStop typeTrailingStop, TradeType typeOrder, double stopLoss, double currentPrice, double enterPrice, Symbol symbol){
            // 1. creo più condizioni per le diverse strategie di trailing stop
            switch (typeTrailingStop)//.ToString())
            {
                case StrategieTrailingStop.TrailingSoloBreakEven:
                    // 1. controllo se l'operazione in corso è in buy o sell così da poter mettere correttamente lo stop loss
                    // BUY 
                    if ( typeOrder == TradeType.Buy ){
                        double stopLossPip = Math.Abs(enterPrice - stopLoss) * (Math.Pow(10, symbol.Digits - 1));
                        // 2. controllo che lo stopLoss sia maggiore di 1 pip dato che se il trailing stop fosse stato già applicato 
                        // lo stop loss è già affianco al prezzo di entrata
                        if ( stopLossPip <= 1){
                            return;
                        }
                        //3. controllo che il prezzo attuale si trova agli stessi pip di distanza dal prezzo di entrata 
                        // come lo stop loss dal prezzo di entrata
                        double targetPrice = enterPrice + Math.Abs(enterPrice - stopLoss); 
                        //Print( "Il targetPrice con la strategia SoloBreakEven è = " + targetPrice + " dal prezzo di entrata = " + enterPrice + " e con stop loss = " + stopLossPip);
                        if ( currentPrice >= targetPrice ){
                            // cambio lo stop loss portando a break-even(per il momento al prezzo di entrata)
                            foreach (var position in Positions)
                            {
                                position.ModifyStopLossPrice(enterPrice);
                                break;
                            }
                            Print("Stop Loss cambiato");
                        }
                    // SELL
                    } else if ( typeOrder == TradeType.Sell ){
                        double stopLossPip = Math.Abs(enterPrice - stopLoss) * (Math.Pow(10, symbol.Digits - 1));
                        // 2. controllo che lo stopLoss sia maggiore di 1 pip dato che se il trailing stop fosse stato già applicato 
                        // lo stop loss è già affianco al prezzo di entrata
                        if ( stopLossPip <= 1){
                            return;
                        }
                        //3. controllo che il prezzo attuale si trova agli stessi pip di distanza dal prezzo di entrata 
                        // come lo stop loss dal prezzo di entrata
                        double targetPrice = enterPrice - Math.Abs(enterPrice - stopLoss); 
                        //Print( "Il targetPrice con la strategia SoloBreakEven è = " + targetPrice + " dal prezzo di entrata = " + enterPrice + " e con stop loss = " + stopLossPip);
                        if ( currentPrice <= targetPrice ){
                            // cambio lo stop loss portando a break-even(per il momento al prezzo di entrata)
                            foreach (var position in Positions)
                            {
                                position.ModifyStopLossPrice(enterPrice);
                                break;
                            }
                            //Print("Stop Loss cambiato");
                        }
                    }
                    break;
                    
                    
                    
                case StrategieTrailingStop.TrailingBreakEvenEPoi:
                    break;
                      
            }
        }
        
        
        // prelva il massimo prezzo tra due date in maniera che controlla dalla candela più recente a quella più vecchia
        public double GetHighestPriceDecr(DateTime startTime, DateTime endTime) {
            var barMinute =  MarketData.GetBars(TimeFrame.Minute);
            var startBarIndex = barMinute.OpenTimes.GetIndexByTime(startTime);
            var endBarIndex = barMinute.OpenTimes.GetIndexByTime(endTime);

            var max = double.MinValue;
            //Print("StartTime GePrice = "+startTime+"EndTime GePrice = "+endTime);
            for (var barIndex = startBarIndex; barIndex <= endBarIndex; barIndex++)
            {
                //Print("barindex = "+barIndex);
                max = Math.Max(barMinute.HighPrices[barIndex], max);
                //Print("Max = "+max);
            }

            return max;
        
        
        }
       
        // prelva il minimo prezzo tra due date in maniera che controlla dalla candela più recente a quella più vecchia
        public double GetLowestPriceDecr(DateTime startTime, DateTime endTime) {
            var barMinute =  MarketData.GetBars(TimeFrame.Minute);
            var startBarIndex = barMinute.OpenTimes.GetIndexByTime(startTime);
            var endBarIndex = barMinute.OpenTimes.GetIndexByTime(endTime);

            var min = double.MaxValue;

            for (var barIndex = startBarIndex; barIndex <= endBarIndex; barIndex++)
            {
                min = Math.Min(barMinute.LowPrices[barIndex], min);
            }

            return min;
        
        
        }
        
        
        
         // questa funzione aggiunge degli orari all'interno di una liste di tuple dove il primo valore della tupla è l'ora iniziale
        // mentre il secondo valore è il minuto iniziale mentre corrispettivamente il terzo e il quarto sono l'ora e il minuto finale
        public List<Tuple<int,int,int,int>> setOrariAltaVolatilità(){
            // per il momento gli orari ad alta volatilità sono :
            //          - 14:30
            //          - 15:00
            //          - 15:30
            //          - 16:00
            //          - 18:00
            List<Tuple<int,int,int,int>> orariAltaVolatilità = new List<Tuple<int,int,int,int>>();
            //09:00 - 09:05
            //orariAltaVolatilità.Add(new Tuple<int, int,int,int>(09,0,09,5));
            //14:30 - 14:35
            orariAltaVolatilità.Add(new Tuple<int, int,int,int>(14,0,14,35));
            //15:00 - 15:15
            orariAltaVolatilità.Add(new Tuple<int, int,int,int>(15,0,15,5));
            //15:30 - 15:35
            //orariAltaVolatilità.Add(new Tuple<int, int,int,int>(15,30,15,35));
            //16:00 - 16:05
            //orariAltaVolatilità.Add(new Tuple<int, int,int,int>(16,0,16,5));
            //18:00 - 18:05
            //orariAltaVolatilità.Add(new Tuple<int, int,int,int>(18,0,18,5));
            return orariAltaVolatilità;
        }
        
        
        
        //*******************************FUNZIONI PER IL FILE CSV DEI DATI DI BACKTEST *********************************************//
        
        //------------------------- PARAMETRI --------------------//
        // !! INFO : TUTTE LE DATE DEVONO ESSERE IN LINEA CON IL FUSO ORARIO ITALIANO(UTC+1)
        // - TIMEFRAME
        // - SIMBOLO
        // - DIREZIONE
        // - LOTTI
        // - GIORNO ENTRATA
        // - GIORNO SETTIMANA ENTRATA
        // - ORARIO ENTRATA
        // - PREZZO DI ENTRATA
        // - STOP LOSS
        // - TAKE PROFIT
        // - GIORNO CHIUSURA
        // - GIORNO SETTIMANA CHIUSURA
        // - ORARIO CHIUSURA
        // - PREZZO DI CHIUSURA
        // - DURATA TRADE
        // - MAX ESPANSIONE % CON SUPERTREND 
        // - COMMISSIONI
        // - SWAP
        // - LORDO
        // - NETTO
        // - RISK REWARD
        
        // questa funzione scrive in un file i dati dei singoli trade del backtest
        public void writeDataEachTradeBacktest(Position actualPosition){
            // da trovare il nome del file che si possa ricondurre al corrispettivo backtest finale 
            DateOnly startBkFormatted = new DateOnly(startDateBacktesting.Year,startDateBacktesting.Month,startDateBacktesting.Day);
            string startBkFormattedSTR = startBkFormatted.Year.ToString()+"_"+startBkFormatted.Month.ToString()+"_"+startBkFormatted.Day.ToString();
            string myfilePathCsv = @"C:\Users\andre\OneDrive\Desktop\Andrea\Trading\Ctrader\Backtesting\StrategiaSuperTrendLiteV2\BacktestHistory"+Symbol+TimeFrame+"-"+startBkFormattedSTR+".csv";            //+++++ STATISTICHE +++++//
            // nomi delle statistiche 
            List<string> tradeStatistics = new List<string>(){"Timeframe","Simbolo","Direzione","Lotti","Giorno Entrata","Giorno Settimana Entrata","Orario Entrata","Prezzo di Entrata","Stop Loss(Pip)","Take Profit(Pip)","Giorno Chiusura","Giorno Settimana Chiusura","Orario Chiusura","Prezzo di Chiusura","Durata trade(sec)","Commissioni","Swap","Lordo","Netto","Risk Reward","Win Rate","Data"};
            string namesAllStatisticsEachTrade = "" + string.Join(",", tradeStatistics.Select(s => $"{s}"));
            string valuesAllStatisticsEachTrade = getValuesAllStatisticsEachTrade(tradeStatistics,actualPosition);
            
            
            // PRIMA VOLTA            
            if (!File.Exists(myfilePathCsv)){// la prima volta che viene creato il file aggiungo le voci delle statistiche del backtesting
                // !!! LA CONDIZIONE QUA SOTTO NON SERVIREBBE DAL MOMENTO CHE OGNI VOLTA valuesAllStatisticsInput NON SARA' MAI VUOTA
                // controllo che ci siano nuovi dati da aggiungere
                if ( valuesAllStatisticsEachTrade.Length > 0) {                
                    Print("valuesAllStatisticsEachTrade FILE NON ESISTE = "+valuesAllStatisticsEachTrade);
                    txtCsvEachTradeBacktest.AppendLine(namesAllStatisticsEachTrade);
                    txtCsvEachTradeBacktest.AppendLine(valuesAllStatisticsEachTrade);                
                    File.AppendAllText(myfilePathCsv, txtCsvBacktestFinale.ToString());//txtCsvEachTradeBacktest.ToString())
                    Print("aggiunto");
                }else {
                    Print("Nessun dato da aggiungere al file di backtest");
                }
                
            }else{ 
                if ( valuesAllStatisticsEachTrade.Length > 0) {                                
                    Print("valuesAllStatisticsEachTrade FILE ESISTE = "+valuesAllStatisticsEachTrade);
                    txtCsvEachTradeBacktest.AppendLine(valuesAllStatisticsEachTrade);
                    //File.AppendAllText(myfilePathCsv,txtCsvBacktestFinale.ToString()); 
                    File.AppendAllText(myfilePathCsv, txtCsvBacktestFinale.ToString());//txtCsvEachTradeBacktest.ToString())
                    Print("aggiunto");
                }else {
                    Print("Nessun dato da aggiungere al file di backtest");
                }
            }
            
        
        }
        
        
         // questa funzione scrive i dati di input del backtest che riguarda cronologia degli ordini eseguiti
        public void writeInputOnBacktestHistory(){
            // da trovare il nome del file che si possa ricondurre al corrispettivo backtest finale 
            DateOnly startBkFormatted = new DateOnly(startDateBacktesting.Year,startDateBacktesting.Month,startDateBacktesting.Day);
            string startBkFormattedSTR = startBkFormatted.Year.ToString()+"_"+startBkFormatted.Month.ToString()+"_"+startBkFormatted.Day.ToString();
            string myfilePathCsv = @"C:\Users\andre\OneDrive\Desktop\Andrea\Trading\Ctrader\Backtesting\StrategiaSuperTrendLiteV2\BacktestHistory"+Symbol+TimeFrame+"-"+startBkFormattedSTR+".csv";            //+++++ STATISTICHE +++++//
            //+++++ INPUT +++++//
            // nomi delle statistiche input
            List<string> tradeStatisticsInput = new List<string>(){"Timeframe","Data inizio","Data fine","GG settimana non operativi","Mesi non operativi","Supertrend period","Supertrend multiplier","Supertrend source","% rischio x trade","Tipo direzione entrata","Modalità entrata","Modalità limite entrata","Modalità uscita","Fine giornata uscita","Orario fine giornata uscita","Massima perdita perc uscita","Modalità stop loss","Misura Stop Loss","Percentuale rischio stop loss","Strategia trailingstop","Extra"};//,"Data"};
            string namesAllStatisticsInput = "" + string.Join(",", tradeStatisticsInput.Select(s => $"{s}"));
            // valori delle statistiche input
            string valuesAllStatisticsInput = getValuesAllStatisticsInput(tradeStatisticsInput);
            txtCsvEachTradeBacktest.AppendLine(namesAllStatisticsInput);
            txtCsvEachTradeBacktest.AppendLine(valuesAllStatisticsInput+"-,-");
            txtCsvEachTradeBacktest.AppendLine("" + string.Join(",", tradeStatisticsInput.Select(s => "-"))+"-,-");
            //Print("Final text each trade = namesAllStatisticsInput "+namesAllStatisticsInput+"\nvaluesAllStatisticsInput = "+ valuesAllStatisticsInput);
            Print("txtCsv = "+txtCsvEachTradeBacktest.ToString());
            File.AppendAllText(myfilePathCsv, txtCsvEachTradeBacktest.ToString());
            Print("scritto qui = "+myfilePathCsv);
        } 
        
        
        // - TIMEFRAME
        // - SIMBOLO
        // - DIREZIONE
        // - LOTTI
        // - GIORNO ENTRATA
        // - GIORNO SETTIMANA ENTRATA
        // - ORARIO ENTRATA
        // - PREZZO DI ENTRATA
        // - STOP LOSS(PIP)
        // - TAKE PROFIT(PIP)
        // - GIORNO CHIUSURA
        // - GIORNO SETTIMANA CHIUSURA
        // - ORARIO CHIUSURA
        // - PREZZO DI CHIUSURA
        // - DURATA TRADE(SEC)
        // - MAX ESPANSIONE % CON SUPERTREND 
        // - COMMISSIONI
        // - SWAP
        // - LORDO
        // - NETTO
        // - RISK REWARD 
        // - WIN RATE
        
        // questa funzione restituisce la differenza in secondi tra la data di inizio del trade e quella attuale(la data di chiusura)
        // ho scelto i secondi dal momento che sono l'unità temporale più piccola e successivamente tramite python convertire in altre unità temporali 
        public string getSecondiDurataTrade(Position pos){
            string durataTrade = "";
            DateTime dataInizio = pos.EntryTime;
            DateTime dataFine = Server.Time;
            TimeSpan diffDate = new TimeSpan();
            // dataInizio è più recente di dataFine
            if(dataInizio.CompareTo(dataFine) == 1){
                diffDate = dataInizio.Subtract(dataFine);
            }
            // dataFine è più recente di dataInizio
            else if(dataFine.CompareTo(dataInizio) == 1){
                diffDate = dataFine.Subtract(dataInizio);
            }
            string secondsDiffTrade = ((int) diffDate.TotalSeconds).ToString();
            Print("Diff Date(secondi) = "+secondsDiffTrade);
            return secondsDiffTrade;
        }
        
        // questa funzione restituisce il Risk-Reward di un trade 
        // il Risk-Reward è calcolato come il rapporto tra il take-profit e lo stop-loss
        // RR = TP/SL 
        public double getRR(Position actualPosition){
            // Take Profit
            double takeProfitPip = getPipTwoPrices(Math.Round(actualPosition.EntryPrice,Symbol.Digits),Math.Round((double)Symbol.Bid,Symbol.Digits));
            // Stop Loss            
            double stopLossPip = getPipTwoPrices(Math.Round(actualPosition.EntryPrice,Symbol.Digits),Math.Round((double)actualPosition.StopLoss,Symbol.Digits));
            // RR
            double riskReward = takeProfitPip/stopLossPip;
            return Math.Round(riskReward,2);
        }
        
        
        // questa funzione restituisce il numero di vittorie
        public int getNVittorie(){
            int nVitt = 0;
            nVitt = History.Select(trade => trade.NetProfit).Where(x => x >= 0).Count();
            Print("NVittorie = "+nVitt);
            return nVitt;
        }
        
        // questa funzione restituisce il winRate attuale calcolando il win rate precedente con la nuova perdita o guadagno
        public double getWinRate(Position actualPosition)
        {
            int nOrdiniTotali = History.Count;
            // se stiamo calcolando il winRate del primo ordine allora sarà o 0% o 100%
            if ( nOrdiniTotali == 0 ){
                double risultatoOrdine = actualPosition.NetProfit;
                // vittoria = 100%
                if ( risultatoOrdine >= 0 ){
                    winRate = 100;
                }
                // perdita = 0%
                else if ( nOrdiniTotali == 0 ){
                     winRate = 0.0;
                }
            }
            // calcolo del winRate dal secondo trade in poi
            else{
                 // calcolo il numero di vittorie precedenti come la percentuale di winRate x Numero Tot di Operazioni precedente
                 //double lastWins = Math.Floor( (winRate/100) * nOrdiniTotali);
                 double lastWins = (double) getNVittorie();
                 double nOrdiniTotAttuale = nOrdiniTotali + 1; // aggiungo +1 dal momento che l'ordine attuale non è ancora chiuso
                 int vittoriaPerdita = 0;//questa variabile assume il valore 1 (= vittoria) alltrimenti 0 (=perdita)
                 double risultatoOrdine = actualPosition.NetProfit;
                 // vittoria = 100%
                 if ( risultatoOrdine >= 0 ){
                     vittoriaPerdita = 1;
                 }
                 // perdita = 0%
                 else if ( nOrdiniTotali == 0 ){
                      vittoriaPerdita = 0;
                 }
                 double actualWins = lastWins + vittoriaPerdita;
                 // il winRate attuale corrisponde alle vittorie attuali diviso il numero totale di ordini attuale
                 winRate = (actualWins/nOrdiniTotAttuale)*100;
                 Print("nOrdiniTotAttuale = "+nOrdiniTotAttuale+"\nLast Wins = "+lastWins+"\nvittoriaPerdita = "+vittoriaPerdita+"\nactualWins= "+actualWins+"\nwinRate = "+winRate+"\nvery value = (actualWins/nOrdiniTotAttuale)= "+actualWins/nOrdiniTotAttuale+"---"+winRate.ToString()); 
            }
            double winRateActual = winRate ;
            return Math.Round(winRateActual,2);
        }
        
        //"Timeframe","Simbolo","Direzione","Lotti","Giorno Entrata","Giorno Settimana Entrata","Orario Entrata","Prezzo di Entrata","Stop Loss(Pip)","Take Profit(Pip)","Giorno Chiusura","Giorno Settimana Chiusura","Orario Chiusura","Prezzo di Chiusura","Durata trade(sec)","Commissioni","Swap","Lordo","Netto","Risk Reward","Win Rate"};
        // questa funzione aggiorna la stringa dove salvo tutti i valori delle statistiche da prelevare per ogni trade
        public string getValuesAllStatisticsEachTrade(List<string> parametersEachTrade,Position actualPosition){
            string namesAllStatisticsEachTrade = "";
            for (int i = 0; i < parametersEachTrade.Count; i++)
            {
                string nameOneStatisticsInput = parametersEachTrade.ElementAt(i);
                // Timeframe
                if ( nameOneStatisticsInput == "Timeframe" ){
                    string timeframeTrade = TimeFrame.ToString();
                    Print("timeframeTrade = "+timeframeTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + timeframeTrade +",";
                }
                // Simbolo
                if ( nameOneStatisticsInput == "Simbolo" ){
                    string simboloTrade = actualPosition.SymbolName.ToString();
                    Print("simboloTrade = "+simboloTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + simboloTrade +",";
                }
                // Direzione
                if ( nameOneStatisticsInput == "Direzione" ){
                    string direzioneTrade = actualPosition.TradeType.ToString();
                    Print("direzioneTrade = "+direzioneTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + direzioneTrade +",";
                }
                // Lotti
                if ( nameOneStatisticsInput == "Lotti" ){
                    string quantitaTrade = actualPosition.Quantity.ToString().Replace(",",".");
                    Print("quantitaTrade = "+quantitaTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + quantitaTrade +",";
                }
                // Giorno Entrata
                if ( nameOneStatisticsInput == "Giorno Entrata" ){
                    DateTime dateOpen = actualPosition.EntryTime.Date;
                    DateOnly dateOnly = new DateOnly(dateOpen.Year,dateOpen.Month,dateOpen.Day);
                    string giornoEntrataTrade = dateOnly.ToString();
                    Print("giornoEntrataTrade = "+giornoEntrataTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + giornoEntrataTrade +",";
                }
                // Giorno Settimana Entrata
                if ( nameOneStatisticsInput == "Giorno Settimana Entrata" ){
                    string giornoSettimanaEntrataTrade = actualPosition.EntryTime.Date.DayOfWeek.ToString();
                    Print("giornoSettimanaEntrataTrade = "+giornoSettimanaEntrataTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + giornoSettimanaEntrataTrade +",";
                }
                // Orario Entrata
                if ( nameOneStatisticsInput == "Orario Entrata" ){
                    TimeSpan timeOpen = actualPosition.EntryTime.TimeOfDay;
                    TimeSpan timeOnly = new TimeSpan(timeOpen.Hours,timeOpen.Minutes,timeOpen.Seconds);
                    string orarioEntrataTrade = timeOnly.ToString(); //getOrarioEntrataEachTrade(actualPosition.EntryTime);
                    Print("orarioEntrataTrade = "+orarioEntrataTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + orarioEntrataTrade +",";
                }                
                // Prezzo di Entrata
                if ( nameOneStatisticsInput == "Prezzo di Entrata" ){
                    string prezzoEntrataTrade = actualPosition.EntryPrice.ToString().Replace(",",".");
                    Print("prezzoEntrataTrade = "+prezzoEntrataTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + prezzoEntrataTrade +",";
                }
                // Stop Loss(Pip)
                if ( nameOneStatisticsInput == "Stop Loss(Pip)" ){
                    double stopLossPip = getPipTwoPrices(Math.Round(actualPosition.EntryPrice,Symbol.Digits),Math.Round((double)actualPosition.StopLoss,Symbol.Digits));
                    string stopLossTrade = stopLossPip.ToString().Replace(",",".");
                    Print("stopLossTrade = "+stopLossTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + stopLossTrade +",";
                }
                // Take Profit(Pip)
                if ( nameOneStatisticsInput == "Take Profit(Pip)" ){
                    double takeProfitPip = getPipTwoPrices(Math.Round(actualPosition.EntryPrice,Symbol.Digits),Math.Round((double)Symbol.Bid,Symbol.Digits));
                    string takeProfitTrade = takeProfitPip.ToString().Replace(",",".");
                    Print("takeProfitTrade = "+takeProfitTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + takeProfitTrade +",";
                }// Giorno Chiusura
                if ( nameOneStatisticsInput == "Giorno Chiusura" ){
                    DateTime dateClose = Server.Time.Date;
                    DateOnly dateOnly = new DateOnly(dateClose.Year,dateClose.Month,dateClose.Day);
                    string giornoChiusuraTrade = dateOnly.ToString();
                    Print("giornoChiusuraTrade = "+giornoChiusuraTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + giornoChiusuraTrade +",";
                }
                // Giorno Settimana Chiusura
                if ( nameOneStatisticsInput == "Giorno Settimana Chiusura" ){
                    string giornoSettimanaChiusuraTrade = Server.Time.Date.DayOfWeek.ToString();
                    Print("giornoSettimanaChiusuraTrade = "+giornoSettimanaChiusuraTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + giornoSettimanaChiusuraTrade +",";
                }
                // Orario Chiusura
                if ( nameOneStatisticsInput == "Orario Chiusura" ){
                    TimeSpan timeClose = Server.Time.TimeOfDay;
                    TimeSpan timeOnly = new TimeSpan(timeClose.Hours,timeClose.Minutes,timeClose.Seconds);
                    string orarioChiusuraTrade = timeOnly.ToString(); //getOrarioEntrataEachTrade(actualPosition.EntryTime);
                    Print("orarioChiusuraTrade = "+orarioChiusuraTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + orarioChiusuraTrade +",";
                }                
                // Prezzo di Chiusura
                if ( nameOneStatisticsInput == "Prezzo di Chiusura" ){
                    string prezzoChiusuraTrade = actualPosition.CurrentPrice.ToString().Replace(",",".");
                    Print("prezzoChiusuraTrade = "+prezzoChiusuraTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + prezzoChiusuraTrade +",";
                }
                // Durata trade
                if ( nameOneStatisticsInput == "Durata trade(sec)" ){
                    string durataTrade = getSecondiDurataTrade(actualPosition);
                    Print("durataTrade = "+durataTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + durataTrade +",";
                }
                // Commissioni
                if ( nameOneStatisticsInput == "Commissioni" ){
                    string commissioniTrade = actualPosition.Commissions.ToString().Replace(",",".");
                    Print("commissioniTrade = "+commissioniTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + commissioniTrade +",";
                }
                // Swap
                if ( nameOneStatisticsInput == "Swap" ){
                    string swapsTrade = actualPosition.Swap.ToString().Replace(",",".");
                    Print("swapsTrade = "+swapsTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + swapsTrade +",";
                }
                // Lordo
                if ( nameOneStatisticsInput == "Lordo" ){
                    string lordoTrade = actualPosition.GrossProfit.ToString().Replace(",",".");
                    Print("lordoTrade = "+lordoTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + lordoTrade +",";
                }
                // Netto
                if ( nameOneStatisticsInput == "Netto" ){
                    string nettoTrade = actualPosition.NetProfit.ToString().Replace(",",".");
                    Print("nettoTrade = "+nettoTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + nettoTrade +",";
                }
                // Risk Reward
                if ( nameOneStatisticsInput == "Risk Reward" ){
                    string riskRewardTrade = getRR(actualPosition).ToString().Replace(",",".");
                    Print("riskRewardTrade = "+riskRewardTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + riskRewardTrade +",";
                }
                // Win Rate
                if ( nameOneStatisticsInput == "Win Rate" ){
                    string winRateTrade = getWinRate(actualPosition).ToString().Replace(",",".");
                    Print("winRateTrade = "+winRateTrade);
                    namesAllStatisticsEachTrade = namesAllStatisticsEachTrade + winRateTrade+"";
                }
            }
            return namesAllStatisticsEachTrade;
            
        }
        
        
        //-----------------------------------------------------------------------------------------------------------------------------------------//
        
        // questa funzione scrive in un apposito file i parametri in input della strategia e i dati in output del backtest 
        public void writeDataOutputBacktest(){
            string myfilePathCsv = @"C:\Users\andre\OneDrive\Desktop\Andrea\Trading\Ctrader\Backtesting\StrategiaSuperTrendLiteV2\BacktestOutput"+Symbol+TimeFrame+".csv";
                        
            //+++++ INPUT +++++//
            // nomi delle statistiche input
            List<string> tradeStatisticsInput = new List<string>(){"Timeframe","Data inizio","Data fine","GG settimana non operativi","Mesi non operativi","Supertrend period","Supertrend multiplier","Supertrend source","% rischio x trade","Tipo direzione entrata","Modalità entrata","Modalità limite entrata","Modalità uscita","Fine giornata uscita","Orario fine giornata uscita","Massima perdita perc uscita","Modalità stop loss","Misura Stop Loss","Percentuale rischio stop loss","Strategia trailingstop"};
            string namesAllStatisticsInput = "" + string.Join(",", tradeStatisticsInput.Select(s => $"{s}"));
            //string namesAllStatisticsInput = getNamesAllStatisticsInput();
            // valori delle statistiche input
            string valuesAllStatisticsInput = getValuesAllStatisticsInput(tradeStatisticsInput);
            
            //+++++ OUTPUT +++++//
            // nomi delle statistiche output  da aggiungere nel futuro : "Drawdown max equity %"; opzionali : Sharpe Ratio ,Sortino Ratio
            List<string> tradeStatisticsOutput = new List<string>(){"Profitto netto","Fattore profitto","Commissioni","Swaps","Saldo max drawdown %","Trade totali","Trade vincenti","Max n° trade consecutivi vincenti","Massimo trade vincente","Trade perdenti","Max n° trade consecutivi perdenti","Massimo trade perdente","Trade medi"};
            string namesAllStatisticsOutput = "," + string.Join(",", tradeStatisticsOutput.Select(s => $"{s}"));
            // valori delle statistiche output
            string valuesAllStatisticsOutput = getValuesAllStatisticsOutput(tradeStatisticsOutput);
            
            //+++++ TUTTI +++++//
            // tutti i nomi delle statistiche input e output
            string namesAllStatistics = namesAllStatisticsInput + namesAllStatisticsOutput;
            // tutti i valori delle statistiche input e output 
            string valuesAllStatistics = valuesAllStatisticsInput + valuesAllStatisticsOutput;
            
            if (!File.Exists(myfilePathCsv)){// la prima volta che viene creato il file aggiungo le voci delle statistiche del backtesting
                //Print("namesAllStatistics = "+namesAllStatistics+"\nvaluesAllStatistics"+valuesAllStatistics);
                // !!! LA CONDIZIONE QUA SOTTO NON SERVIREBBE DAL MOMENTO CHE OGNI VOLTA valuesAllStatisticsInput NON SARA' MAI VUOTA
                // controllo che ci siano nuovi dati da aggiungere
                if ( valuesAllStatistics.Length > 0) {                
                    //Print("File non esiste aggiunto    len valuesAllStatistics"+valuesAllStatistics.Length);
                    txtCsvBacktestFinale.AppendLine(namesAllStatistics);
                    txtCsvBacktestFinale.AppendLine(valuesAllStatistics);                
                    File.WriteAllText(myfilePathCsv, txtCsvBacktestFinale.ToString());
                    Print("aggiunto");
                }else {
                    Print("Nessun dato da aggiungere al file di backtest");
                }
                
            }else{ // le successive volte aggiungo i valori delle statistiche del backtesting                 
                // controllo che ci siano nuovi dati da aggiungere
                // !!! LA CONDIZIONE QUA SOTTO NON SERVIREBBE DAL MOMENTO CHE OGNI VOLTA valuesAllStatisticsInput NON SARA' MAI VUOTA
                if ( valuesAllStatistics.Length > 0) {                                
                    txtCsvBacktestFinale.AppendLine(valuesAllStatistics);
                    File.AppendAllText(myfilePathCsv,txtCsvBacktestFinale.ToString());                    
                    Print("aggiunto");
                }else {
                    Print("Nessun dato da aggiungere al file di backtest");
                }
            }
            
            //Trade
            Print("WriteDataOutputBacktest");
        }
        
        
        
           
        
        
        

        //------------------------- INPUT --------------------//
        // ++++++++ DATI DI INPUT DA SALVARE ++++++++++//
        // - TIMEFRAME
        // - DATA INIZIO
        // - DATA FINE
        // - GIORNO SETTIMANA NON OPERATIVI
        // - MESI NON OPERATIVI
        // - SUPETREND PERIOD
        // - SUPETREND MULTIPLIER
        // - SUPETREND SOURCE
        // - % RISCHIO PER OGNI TRADE
        // - TIPO DIREZIONE DI ENTRATA
        // - MODALITA' ENTRATA
        // - ORA INIZIO OPERATIVITA'ENTRATA
        // - ORA FINE OPERATIVITA' ENTRATA
        // - MODALITA' LIMITE ENTRATA
        // - ALTA VOLATILITA' ENTRATA
        // - SPREAD ALTO NOTTE ENTRATA
        // - MODALITA' USCITA
        // - FINE GIORNATA USCITA
        // - ORARIO FINE GIORNATA USCITA
        // - MASSIMA PERDITA PERCENTUALE USCITA
        // - ALTA VOLATILITA' USCITA
        // - SPREAD ALTO NOTTE USCITA
        // - MODALITA' STOP LOSS
        // - PERCENTUALE RISCHIO STOP LOSS
        // - STRATEGIA TRAILINGSTOP STOP LOSS
        
        
        public string getNamesAllStatisticsInput(){
            RobotType myStrategiaSuperTrend = (RobotType) AlgoRegistry.Get("StrategiaSuperTrendLiteV2");
            cAlgo.API.Collections.IReadonlyList<AlgoParameter> parameters = myStrategiaSuperTrend.Parameters;            
            string namesAllStatisticsInput = "";
            
            foreach (AlgoParameter parameter in parameters)
            {
                string nameOneStatisticsInput = parameter.Name;
                // caso nel quale voglio scrivere il nome del parametro Mesi non operativi quindi non devo aggiungere i parametri che fanno meseOperatività
                if ( nameOneStatisticsInput == "gennaioOperatività"){                    
                    namesAllStatisticsInput = namesAllStatisticsInput + "Mesi non operativi" + ",";
                }
                else if (!nameOneStatisticsInput.Contains("Operatività")){
                    namesAllStatisticsInput = namesAllStatisticsInput + nameOneStatisticsInput + ",";
                }
            }
            return namesAllStatisticsInput;
        }
        
        
        
        public string getGGSettimanaNonOperativi(){
            string ggSettNonOp = "";
            List<string> ggSettimana = new List<string>(){"lunedì","martedì","mercoledì","giovedì","venerdì"};
            List<bool> ggSettimanaOperatività = new List<bool>(){isLunedi,isMartedi,isMercoledi,isGiovedi,isVenerdi};
            for (int i = 1; i < 6; i++)
            {
                if ( ggSettimanaOperatività.ElementAt(i-1) == false)
                {
                    ggSettNonOp = ggSettNonOp + " "+ ggSettimana.ElementAt(i-1);
                }
            }
            if ( ggSettNonOp == ""){
                ggSettNonOp = "Nessuno";
            }
            return ggSettNonOp;        
        }
        
        
        public string getMesiNonOperativi(){
            string mesiNonOp = "";
            List<string> mesi = new List<string>(){"gennaio","febbraio","marzo","aprile","maggio","giugno","luglio","agosto","settembre","ottobre","novembre","dicembre"};
            List<bool> mesiOperatività = new List<bool>(){gennaioOperatività,febbraioOperatività,marzoOperatività,aprileOperatività,maggioOperatività,giugnoOperatività,luglioOperatività,agostoOperatività,settembreOperatività,ottobreOperatività,novembreOperatività,dicembreOperatività};
            for (int i = 1; i < 13; i++)
            {
                if ( mesiOperatività.ElementAt(i-1) == false)
                {
                    mesiNonOp = mesiNonOp + " "+ mesi.ElementAt(i-1);
                }
            }
            if ( mesiNonOp == ""){
                mesiNonOp = "Nessuno";
            }
            return mesiNonOp;        
        }
        
        
        
        public string getValuesAllStatisticsInput(List<string> parametersInput){
            List<string> tradeStatisticsInput = new List<string>(){};
            RobotType myStrategiaSuperTrend = (RobotType) AlgoRegistry.Get("StrategiaSuperTrendLiteV2");
            cAlgo.API.Collections.IReadonlyList<AlgoParameter> parameters = myStrategiaSuperTrend.Parameters;
            // "Timeframe","Data inizio","Data fine","GG settimana non operativi","Mesi non operativi",
            // "Supertrend period","Supertrend multiplier","Supertrend source",
            //"% rischio x trade",
            //"Tipo direzione entrata","Modalità entrata","Modalità limite entrata",
            // "Modalità uscita","Fine giornata uscita","Orario fine giornata uscita","Massima perdita perc uscita",
            //"Modalità stop loss","Percentuale rischio stop loss","Strategia trailingstop"
            string valuesAllStatisticsInput = "";
            
            for (int i = 0; i < parametersInput.Count; i++)
            {
                string nameOneStatisticsInput = parametersInput.ElementAt(i);
                //Print("nameOneStatisticsInput = "+nameOneStatisticsInput);
                // Timeframe
                if ( nameOneStatisticsInput == "Timeframe" ){
                    string timeframe = TimeFrame.ToString();
                    valuesAllStatisticsInput = valuesAllStatisticsInput + timeframe +",";
                }
                // Data inizio
                if ( nameOneStatisticsInput == "Data inizio" ){
                    string dataInizioBacktest = startDateBacktesting.ToString();
                    valuesAllStatisticsInput = valuesAllStatisticsInput + dataInizioBacktest +",";
                }
                // Data fine
                if ( nameOneStatisticsInput == "Data fine" ){
                    string datFineBacktest = endDateBacktesting.ToString();
                    valuesAllStatisticsInput = valuesAllStatisticsInput + datFineBacktest +",";
                }
                // Giorno settimana non operativi
                if ( nameOneStatisticsInput == "GG settimana non operativi" ){
                    //Print("GetGiornoSettNonOp = "+ getGGSettimanaNonOperativi());
                    valuesAllStatisticsInput = valuesAllStatisticsInput + getGGSettimanaNonOperativi() +",";
                }
                // Mesi non operativi
                if ( nameOneStatisticsInput == "Mesi non operativi"){ // farò i controllo degli altri mesi attraverso il metodo getMesiNonOperativi()
                    //Print("GetMesiNonOp = "+ getMesiNonOperativi());
                    valuesAllStatisticsInput = valuesAllStatisticsInput + getMesiNonOperativi() +",";
                }
                
                // Supertrend period
                if ( nameOneStatisticsInput == "Supertrend period" ){
                    string superTrendPeriod = SuperPeriod.ToString().Replace(",",".");
                    valuesAllStatisticsInput = valuesAllStatisticsInput + superTrendPeriod + ",";
                }
                // Supertrend multiplier
                if ( nameOneStatisticsInput == "Supertrend multiplier" ){
                    string superTrendMulti = SuperMulti.ToString().Replace(",",".");
                    valuesAllStatisticsInput = valuesAllStatisticsInput + "- "+ superTrendMulti + "-,";
                }
                // Supertrend source
                if ( nameOneStatisticsInput == "Supertrend source" ){
                    string superTrendSource = SuperSource.ToString();
                    valuesAllStatisticsInput = valuesAllStatisticsInput + superTrendSource + ",";
                }
                // % rischio x trade
                if ( nameOneStatisticsInput == "% rischio x trade" ){
                    string rischioPerOgniTrade = riskPerc.ToString().Replace(",",".");
                    valuesAllStatisticsInput = valuesAllStatisticsInput + "- "+rischioPerOgniTrade + " -,";
                }
                // Tipo direzione entrata
                if ( nameOneStatisticsInput == "Tipo direzione entrata" ){
                    string tipoDirEntrata = dirOrdine.ToString();
                    valuesAllStatisticsInput = valuesAllStatisticsInput + tipoDirEntrata + ",";
                }
                // Modalità entrata
                if ( nameOneStatisticsInput == "Modalità entrata" ){
                    string modalitaEntrata = modEntrataOrd.ToString();
                    valuesAllStatisticsInput = valuesAllStatisticsInput + modalitaEntrata + ",";
                }
                // Modalità limite entrata
                if ( nameOneStatisticsInput == "Modalità limite entrata" ){
                    string modalitaLimEntrata = limEntrataOrdine.ToString();
                    valuesAllStatisticsInput = valuesAllStatisticsInput+ modalitaLimEntrata +",";
                }
                // Modalità uscita
                if ( nameOneStatisticsInput == "Modalità uscita" ){
                    string modalitaUscita = modUscitaOrd.ToString();
                    valuesAllStatisticsInput = valuesAllStatisticsInput + modalitaUscita +",";
                }
                // Fine giornata uscita
                if ( nameOneStatisticsInput == "Fine giornata uscita" ){
                    string fineGiornataUscita = chiusuraFineGiornata.ToString();
                    valuesAllStatisticsInput = valuesAllStatisticsInput + fineGiornataUscita +",";
                }
                // Orario fine giornata uscita
                if ( nameOneStatisticsInput == "Orario fine giornata uscita" ){
                    string oraFineGiornataUscita = oraChiusuraFineGiornata.ToString();                    
                    string minutiFineGiornataUscita = minutiChiusuraFineGiornata.ToString();
                    valuesAllStatisticsInput = valuesAllStatisticsInput + oraFineGiornataUscita +":"+ minutiFineGiornataUscita +",";
                }
                // Massima perdita perc uscita
                if ( nameOneStatisticsInput == "Massima perdita perc uscita" ){
                    string massimaPerdPercUscita = isMaxPerditaPerc.ToString();
                    valuesAllStatisticsInput = valuesAllStatisticsInput + massimaPerdPercUscita +",";
                }
                // Modalità stop loss
                if ( nameOneStatisticsInput == "Modalità stop loss" ){
                    string modalitaStopLoss = modStopLossOrd.ToString();
                    valuesAllStatisticsInput = valuesAllStatisticsInput + modalitaStopLoss +",";
                }
                
                // Misura Stop Loss
                if ( nameOneStatisticsInput == "Misura Stop Loss" ){
                    string misuraStopLoss = misuraSL.ToString();
                    valuesAllStatisticsInput = valuesAllStatisticsInput + misuraStopLoss +",";
                }
                // Percentuale rischio stop loss
                if ( nameOneStatisticsInput == "Percentuale rischio stop loss" ){
                    string percentualeRiskStopLoss = stopLossPercRisk.ToString();
                    valuesAllStatisticsInput = valuesAllStatisticsInput + percentualeRiskStopLoss +",";
                }
                // Strategia trailingstop
                if ( nameOneStatisticsInput == "Strategia trailingstop" ){
                    string strTrailingStop = strategiaTrailingStop.ToString();
                    valuesAllStatisticsInput = valuesAllStatisticsInput + strTrailingStop +",";
                }
            }
            return valuesAllStatisticsInput;
        }
        
        //------------------------- OUTPUT --------------------//
        // ++++++++ DATI DI OUTPUT DA SALVARE ++++++++++//
        // !!! TUTTI I PUNTI SI RIFERISCONO A TUTTI,LONG,SHORT
        // - PROFITTO NETTO 
        // - FATTORE PROFITTO
        // - COMMISSIONE
        // - SWAP
        // - SALDO MAX DRAWDOWN
        // - DRAWDOWN MAX EQUITY
        // - OPERAZIONI TOTALI
        // - VINCITE MAX CONSECUTIVE
        // - MAGGIOR TRADE VINCENTE
        // - OPERAZIONI PERDENTI
        // - PERDITE MAX CONSECUTIVE
        // - MAGGIOR TRADE PERDENTE
        // - TRADE MEDI
        //**** OPZIONALI
        // - SHAPE RATIO
        // - SORTINO RATIO
        
        
        public string getValuesAllStatisticsOutput(List<string> nameAllStatisticsOutput){
            //List<string> tradeStatistics = new List<string>(){"Profitto netto","Fattore profitto","Commissioni","Swaps","Trade totali","Trade vincenti","Max n° trade consecutivi vincenti","Massimo trade vincente","Trade perdenti","Max n° trade consecutivi perdenti","Massimo trade perdente","Trade Medio"};
            string valuesAllStatisticsOutput = "";
            // controllo per prima che ci sia almeno un operazione fatta quindi presente nella History
            if ( History.Count > 0 ){            
                for (int i = 0; i < nameAllStatisticsOutput.Count; i++)
                {
                    string nameOneStatisticsOutput = nameAllStatisticsOutput.ElementAt(i);
                    Print("nameOneStatisticsOutput = "+nameOneStatisticsOutput);
                    // Profitto netto
                    if ( nameOneStatisticsOutput == "Profitto netto" ){
                        string ProfittoNettoTot = Extractor.GetNetProfit(this.History.Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        string ProfittoNettoLong = Extractor.GetNetProfit(this.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        string ProfittoNettoShort = Extractor.GetNetProfit(this.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        valuesAllStatisticsOutput = valuesAllStatisticsOutput + string.Format("Tot: {0};Long: {1};Short: {2},", ProfittoNettoTot, ProfittoNettoLong, ProfittoNettoShort);
                    }
                    // Fattore Profitto
                    if ( nameOneStatisticsOutput == "Fattore profitto" ){
                        string FattoreProfittoTot = Extractor.GetProfitFactor(this.History.Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        string FattoreProfittoLong = Extractor.GetProfitFactor(this.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        string FattoreProfittoShort = Extractor.GetProfitFactor(this.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        valuesAllStatisticsOutput = valuesAllStatisticsOutput + string.Format("Tot: {0};Long: {1};Short: {2},", FattoreProfittoTot, FattoreProfittoLong, FattoreProfittoShort);
                    }
                    // Commissioni
                    if ( nameOneStatisticsOutput == "Commissioni" ){
                        string CommissioniTot = Extractor.GetCommissions(this.History.Select(trade => trade.Commissions)).ToString().Replace(",",".");
                        string CommissioniLong = Extractor.GetCommissions(this.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.Commissions)).ToString().Replace(",",".");
                        string CommissioniShort = Extractor.GetCommissions(this.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.Commissions)).ToString().Replace(",",".");
                        valuesAllStatisticsOutput = valuesAllStatisticsOutput + string.Format("Tot: {0};Long: {1};Short: {2},", CommissioniTot, CommissioniLong, CommissioniShort);
                    }
                    // Swaps
                    if ( nameOneStatisticsOutput == "Swaps" ){
                        string SwapsTrades = Extractor.GetSwaps(this.History.Select(trade => trade.Swap)).ToString().Replace(",",".");
                        string SwapsLong = Extractor.GetSwaps(this.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.Swap)).ToString().Replace(",",".");
                        string SwapsShort = Extractor.GetSwaps(this.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.Swap)).ToString().Replace(",",".");
                        valuesAllStatisticsOutput = valuesAllStatisticsOutput + string.Format("Tot: {0};Long: {1};Short: {2},", SwapsTrades, SwapsLong, SwapsShort);
                    }
                    // Saldo Max Drawdown %
                    if ( nameOneStatisticsOutput == "Saldo max drawdown %" ){                    
                        //string SaldoMaxDrawdownPerc = GetMaxBalanceDrawDown(this.History.Select(trade => trade.Balance),true).ToString().Replace(",",".");
                        string SaldoMaxDrawdownPerc = GetVeryMaxBalanceDrawDown(this.History.Select(trade => trade.Balance),true).ToString().Replace(",",".");
                        //Print("SaldoMaxDrawdownPerc = "+SaldoMaxDrawdownPerc);
                        valuesAllStatisticsOutput = valuesAllStatisticsOutput + "Tot: "+SaldoMaxDrawdownPerc + ",";
                    }
                    // Drawdown Max Equity
                    /*if ( nameOneStatisticsOutput == "Drawdown max equity %" ){
                        string DrawdownMaxEquity = GetMaxEquityDrawDown(true).ToString().Replace(",",".");
                        //valuesAllStatisticsOutput = valuesAllStatisticsOutput + string.Format("{0},", DrawdownMaxEquity);
                    }*/
                    
                    // Trade Totali
                    if ( nameOneStatisticsOutput == "Trade totali" ){
                        string TradeTotali = Extractor.GetTrades(this.History.Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        string TradeTotaliLong = Extractor.GetTrades(this.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        string TradeTotaliShort = Extractor.GetTrades(this.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        valuesAllStatisticsOutput = valuesAllStatisticsOutput + string.Format("Tot: {0} Long: {1} Short: {2},", TradeTotali, TradeTotaliLong, TradeTotaliShort);
                    }
                    // Trade vincenti
                    if ( nameOneStatisticsOutput == "Trade vincenti" ){
                        string TradeVincentiTot = Extractor.GetWiningTrades(this.History.Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        string TradeVincentiLong = Extractor.GetWiningTrades(this.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        string TradeVincentiShort = Extractor.GetWiningTrades(this.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        valuesAllStatisticsOutput = valuesAllStatisticsOutput + string.Format("Tot: {0} Long: {1} Short: {2},", TradeVincentiTot, TradeVincentiLong, TradeVincentiShort);
                    }
                    // Max n° trade consecutivi vincenti
                    if ( nameOneStatisticsOutput == "Max n° trade consecutivi vincenti" ){
                        string MaxNTradeConsecutiviVincentiTot = Extractor.MaxConsecutiveWin(this,"Trades").ToString().Replace(",",".");
                        string MaxNTradeConsecutiviVincentiLong = Extractor.MaxConsecutiveWin(this,"Long").ToString().Replace(",",".");
                        string MaxNTradeConsecutiviVincentiShort = Extractor.MaxConsecutiveWin(this,"Short").ToString().Replace(",",".");
                        valuesAllStatisticsOutput = valuesAllStatisticsOutput + string.Format("Tot: {0} Long: {1} Short: {2},", MaxNTradeConsecutiviVincentiTot, MaxNTradeConsecutiviVincentiLong, MaxNTradeConsecutiviVincentiShort);
                    }
                    // Massimo trade vincente
                    if ( nameOneStatisticsOutput == "Massimo trade vincente" ){
                        string MaxTradeVincenteTot = Extractor.LargesWiningTrade(this.History.Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        string MaxTradeVincenteLong = Extractor.LargesWiningTrade(this.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        string MaxTradeVincenteShort = Extractor.LargesWiningTrade(this.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        valuesAllStatisticsOutput = valuesAllStatisticsOutput + string.Format("Tot: {0} Long: {1} Short: {2},", MaxTradeVincenteTot, MaxTradeVincenteLong, MaxTradeVincenteShort);
                    }
                    // Trade perdenti
                    if ( nameOneStatisticsOutput == "Trade perdenti" ){
                        string TradePerdentiTot = Extractor.GetLosingTrades(this.History.Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        string TradePerdentiLong = Extractor.GetLosingTrades(this.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        string TradePerdentiShort = Extractor.GetLosingTrades(this.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        //Print("TradePerdentiTot = " +TradePerdentiTot+" TradePerdentiLong = "+TradePerdentiLong+" TradePerdentiShort = "+TradePerdentiShort);
                        valuesAllStatisticsOutput = valuesAllStatisticsOutput + string.Format("Tot: {0} Long: {1} Short: {2},", TradePerdentiTot, TradePerdentiLong, TradePerdentiShort);
                    }
                    // Max n° trade consecutivi perdenti
                    if ( nameOneStatisticsOutput == "Max n° trade consecutivi perdenti" ){
                        string MaxNTradeConsecutiviPerdentiTot = Extractor.MaxConsecutiveLoss(this,"Trades").ToString().Replace(",",".");
                        string MaxNTradeConsecutiviPerdentiLong = Extractor.MaxConsecutiveLoss(this,"Long").ToString().Replace(",",".");
                        string MaxNTradeConsecutiviPerdentiShort = Extractor.MaxConsecutiveLoss(this,"Short").ToString().Replace(",",".");
                        valuesAllStatisticsOutput = valuesAllStatisticsOutput + string.Format("Tot: {0} Long: {1} Short: {2},", MaxNTradeConsecutiviPerdentiTot, MaxNTradeConsecutiviPerdentiLong, MaxNTradeConsecutiviPerdentiShort);
                    }
                    // Massimo trade perdente
                    if ( nameOneStatisticsOutput == "Massimo trade perdente" ){
                        string MaxTradePerdenteTot = Extractor.LargestLosingTrade(this.History.Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        string MaxTradePerdenteLong = Extractor.LargestLosingTrade(this.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        string MaxTradePerdenteShort = Extractor.LargestLosingTrade(this.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        valuesAllStatisticsOutput = valuesAllStatisticsOutput + string.Format("Tot: {0} Long: {1} Short: {2},", MaxTradePerdenteTot, MaxTradePerdenteLong, MaxTradePerdenteShort);
                    }
                    // Trade Medio
                    if ( nameOneStatisticsOutput == "Trade medi" ){
                        string TradeMedioTot = Extractor.AverageTrade(this.History.Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        string TradeMedioLong = Extractor.AverageTrade(this.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        string TradeMedioShort = Extractor.AverageTrade(this.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit)).ToString().Replace(",",".");
                        valuesAllStatisticsOutput = valuesAllStatisticsOutput + string.Format("Tot: {0} Long: {1} Short: {2},", TradeMedioTot, TradeMedioLong, TradeMedioShort);
                    }
                    
                    
                    /*var sharpeRatio = SharpeSortino(false, Bot.History.Select(trade => trade.NetProfit));
                    var sharpeRatioLong = SharpeSortino(false, Bot.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit));
                    var sharpeRatioShort = SharpeSortino(false, Bot.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit));
        
                    backtestinresult += string.Format("Sharpe Ratio,{0},{1},{2}\n", sharpeRatio, sharpeRatioLong, sharpeRatioShort);
        
                    var sortinoRatio = SharpeSortino(true, Bot.History.Select(trade => trade.NetProfit));
                    var sortinoRatioLong = SharpeSortino(true, Bot.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit));
                    var sortinoRatioShort = SharpeSortino(true, Bot.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit));
        
                    backtestinresult += string.Format("Sortino Ratio,{0},{1},{2}\n", sortinoRatio, sortinoRatioLong, sortinoRatioShort);
                    */
                }
            }
            return valuesAllStatisticsOutput;
        }
        
        
        
        // restituisce la differenza in percentuale tra un valore massimo e un minimo
        public double differenceBalanceDrawDownToPct(double max, double min )
        {
            var difference = max - min;

            var pct = difference / max * 100;

            return pct;
        }
        // restituisce la differenza assoluta tra un valore massimo e un minimo
        public double differenceBalanceDrawDownAbsolute(double max, double min )
        {
            return max - min;
        }
        
        
        public double GetVeryMaxBalanceDrawDown(IEnumerable<double> vals,bool isPct)
            {
                double maxBalanceDrawDown = 0.0;
                double maxValueBalance = 0.0;
                for (int i = 0; i < vals.Count(); i++)
                {                    
                    double actualBalance = vals.ElementAt(i);
                    // per prima cosa vedo se il bilancio attuale è superiore del massimo bilancio fino a quel momento
                    if ( actualBalance >= maxValueBalance ){
                        maxValueBalance = actualBalance;
                    }                    
                    // vuol dire che il bilancio è sceso quindi sta supendo un drawdown
                    else{
                        double actualDrawDown = 0.0;
                        if (isPct){
                            actualDrawDown = differenceBalanceDrawDownToPct(maxValueBalance,actualBalance);
                        }else{
                            actualDrawDown = differenceBalanceDrawDownAbsolute(maxValueBalance,actualBalance);
                        }
                        // adesso vedo se il drawdown è più alto del massimo drawdown % fino a quel momento
                        if ( actualDrawDown >= maxBalanceDrawDown ){
                            maxBalanceDrawDown = actualDrawDown;
                        }
                    }
                }
                
                return (Math.Round(maxBalanceDrawDown, 2));
         }
        
        
        
        
        
        
        
        public double GetMaxBalanceDrawDown(IEnumerable<double> vals,bool isPct)
            {
                double maxBalanceDrawDown = 0.0;
                double minimoValueBalance = vals.Min();                
                if (isPct){
                    maxBalanceDrawDown = Extractor.MaxEquityBalanceDrawDownToPct(initialBalance,minimoValueBalance);
                    //Print("startBalance = "+initialBalance+" minimoValueBalance = "+minimoValueBalance+" maxBalanceDrawDown = "+maxBalanceDrawDown);
                }else{
                    maxBalanceDrawDown = Extractor.MaxEquityBalanceDrawDown(initialBalance,minimoValueBalance);
                }
                
                return (Math.Round(maxBalanceDrawDown, 2));
         }
         
         public double GetMaxEquityDrawDown(bool isPct)
            {
                double maxEquityDrawDown = 0.0;
                double actualEquity = 0;
                double minimoValueEquity = 0;                
                if (isPct){
                    maxEquityDrawDown = Extractor.MaxEquityBalanceDrawDownToPct(initialEquity,minimoValueEquity);
                    //Print("initialEquity = "+initialEquity+" minimoValueBalance = "+minimoValueEquity+" maxBalanceDrawDown = "+minimoValueEquity);
                }else{
                    maxEquityDrawDown = Extractor.MaxEquityBalanceDrawDown(initialEquity,minimoValueEquity);
                }
                
                return (Math.Round(maxEquityDrawDown, 2));
         }
        
        
        //************++++++++++++++++++++++***********++++++ CLASSE INTERNA PER BACKTEST +++++++++++++*********++++++++++++++*********//
        
        
        
        public class CBotPerformanceDataExtractor : Robot
        {
    
            private Robot Bot;
    
            private string CurrencyName;
    
            public int OptimizationPassId;
    
            public string FullPath;
    
            public string Filename;
    
            public string header;
    
            public List<string> parametersValue;
    
            public int TradeNumber;
    
            public double EquityResult;
    
            public double BalanceResult;
    
            public double NetProfitResult;
    
            public int TradesResult;
    
            public int WinningTradesResult;
    
            public int LosingTradesResult;
    
            public double ProfitFactorResult;
    
            public double SharpeRatioResult;
    
            public double SortinoRatioResult;
    
            public double AverageTradeResult;
    
            public double MaxBalance = 0.0;
    
            public double MaxBalanceDrawDownPct = 0.0;
    
            public double MaxBalanceDrawDown = 0.0;
    
            public double MaxEquity = 0.0;
    
            public double MaxEquityDrawDownPct = 0.0;
    
            public double MaxEquityDrawDown = 0.0;
    
            public string SymbolTimeFrame;
    
            public static Dictionary<string, string> Currencies = new Dictionary<string, string> {
            {"AUD", "$"}, {"CHF", "fr."},{"EUR", "€"},{"GBP", "£"}, {"HKD", "HK$"}, {"NZD", "$"}, {"SEK","kr"},{"USD","$"},
            {"ZAR","R"},{"CAD","$"},{"CNY","¥"},{"MXN","$"},{"SGD","$"},{"JPY","¥"}};
            
            
            
            public void InitBacktesting(Robot bot,string nameAllStatistics, string valueAllStatistics, string symbol, string timeframe,StringBuilder txtCsv){
            
                string myfilePathCsv = @"C:\Users\andre\OneDrive\Desktop\Andrea\Trading\Ctrader\Backtesting\StrategiaSuperTrendLiteV2\BacktestOutput"+symbol+timeframe+".csv";
                
                if (!File.Exists(myfilePathCsv)){// la prima volta che viene creato il file aggiungo le voci delle statistiche del backtesting
                    txtCsv.AppendLine(nameAllStatistics);
                }else{ // le successive volte aggiungo i valori delle statistiche del backtesting
                    txtCsv.AppendLine(valueAllStatistics);
                }
                File.WriteAllText(myfilePathCsv, txtCsv.ToString());
            }
            
            
            
    
            public void InitOptimization(Robot bot, string filename, List<string> parametersname, List<string> pvals)
            {
    
                Bot = bot;
    
                SetSymbolTimeFrame();
    
                parametersValue = pvals;
    
                FullPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + filename + ".csv";
    
                if (Currencies.ContainsKey(Bot.Account.Asset.Name))
                {
                    string val = Currencies[Bot.Account.Asset.Name];
                    CurrencyName = val;
                }
                else
                    CurrencyName = Bot.Account.Asset.Name;
    
                header = "Optimization Pass,Symbol TimeFrame,Equity,Balance,Netprofit,Trades,Winning Trades,Losing Trades,Profit Factor,Max Equity DD (%),Max Balance DD (%),";
    
                header += string.Format("Max Equity DD ({0}),Max Balance DD ({1}),Sharpe,Sortino,Average Trade", CurrencyName, CurrencyName);
    
                bool isEmpty = !parametersname.Any();
                if (!isEmpty)
                {
                    var result = "," + string.Join(",", parametersname.Select(s => $"{s}"));
    
                    header += result;
                }
            }
    
            public void InitBacktesting(Robot bot, string filename)
            {
                Bot = bot;
    
                FullPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + filename + ".csv";
    
                header = "Summary,All Trades,Long Trades,Short Trades";
                File.AppendAllText(FullPath, header + Environment.NewLine, Encoding.UTF8);
    
            }
            public void WriteBacktestingResult()
            {
                var NetProfitTrades = GetNetProfit(Bot.History.Select(trade => trade.NetProfit));
                var NetProfitLong = GetNetProfit(Bot.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit));
                var NetProfitShort = GetNetProfit(Bot.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit));
    
                var backtestinresult = string.Format("Net Profit,{0},{1},{2}\n", NetProfitTrades, NetProfitLong, NetProfitShort);
    
                var ProfitFactorTrades = GetProfitFactor(Bot.History.Select(trade => trade.NetProfit));
                var ProfitFactorLong = GetProfitFactor(Bot.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit));
                var ProfitFactorShort = GetProfitFactor(Bot.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit));
    
                backtestinresult += string.Format("Profit Factor,{0},{1},{2}\n", ProfitFactorTrades, ProfitFactorLong, ProfitFactorShort);
    
                var CommissionsTrades = GetCommissions(Bot.History.Select(trade => trade.Commissions));
                var CommissionsLong = GetCommissions(Bot.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.Commissions));
                var CommissionsShort = GetCommissions(Bot.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.Commissions));
    
                backtestinresult += string.Format("Commission,{0},{1},{2}\n", CommissionsTrades, CommissionsLong, CommissionsShort);
    
    
                backtestinresult += string.Format("Max Balance Draw Down %,{0},{1},{2}\n", MaxBalanceDrawDownPct, "-", "-");
                backtestinresult += string.Format("Max Equity Draw Down %,{0},{1},{2}\n", MaxEquityDrawDownPct, "-", "-");
    
                var TotalTrades = GetTrades(Bot.History.Select(trade => trade.NetProfit));
                var TotalTradesLong = GetTrades(Bot.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit));
                var TotalTradesShort = GetTrades(Bot.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit));
    
                backtestinresult += string.Format("Total Trades,{0},{1},{2}\n", TotalTrades, TotalTradesLong, TotalTradesShort);
    
                var totalWinningTrades = GetWiningTrades(Bot.History.Select(trade => trade.NetProfit));
                var longWinningTrades = GetWiningTrades(Bot.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit));
                var shorWinningTrades = GetWiningTrades(Bot.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit));
    
                backtestinresult += string.Format("Winning Trades,{0},{1},{2}\n", totalWinningTrades, longWinningTrades, shorWinningTrades);
    
                var MaxConsWinTrades = MaxConsecutiveWin(Bot,"Trades");
                var MaxConsWinLongTrades = MaxConsecutiveWin(Bot,"Long");
                var MaxConsWinShortTrades = MaxConsecutiveWin(Bot,"Short");
    
                backtestinresult += string.Format("Max Consecutive Winning Trades,{0},{1},{2}\n", MaxConsWinTrades, MaxConsWinLongTrades, MaxConsWinShortTrades);
    
                var largestWinningTrade = LargesWiningTrade(Bot.History.Select(trade => trade.NetProfit));
                var largetWinningLongTrade = LargesWiningTrade(Bot.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit));
                var largestWinningShortTrade = LargesWiningTrade(Bot.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit));
    
                backtestinresult += string.Format("Largest Winning Trade,{0},{1},{2}\n", largestWinningTrade, largetWinningLongTrade, largestWinningShortTrade);
    
                var LosingTrades = GetLosingTrades(Bot.History.Select(trade => trade.NetProfit));
                var LonglosingTrades = GetLosingTrades(Bot.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit));
                var ShortlosingTades = GetLosingTrades(Bot.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit));
    
                backtestinresult += string.Format("LosingTrades,{0},{1},{2}\n", LosingTrades, LonglosingTrades, ShortlosingTades);
    
                var MaxConsLossTrades = MaxConsecutiveLoss(Bot,"Trades");
                var MaxConsLongLossTrades = MaxConsecutiveLoss(Bot,"Long");
                var MaxConsShortLossTrades = MaxConsecutiveLoss(Bot,"Short");
    
                backtestinresult += string.Format("Max Consecutive Losing Trades,{0},{1},{2}\n", MaxConsLossTrades, MaxConsLongLossTrades, MaxConsShortLossTrades);
    
                var largestLosingTrade = LargestLosingTrade(Bot.History.Select(trade => trade.NetProfit));
                var largestLosingLongTrade = LargestLosingTrade(Bot.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit));
                var largestLosingShortTrade = LargestLosingTrade(Bot.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit));
    
                backtestinresult += string.Format("Largest Losing Trade,{0},{1},{2}\n", largestLosingTrade, largestLosingLongTrade, largestLosingShortTrade);
    
                var averageTrade = AverageTrade(Bot.History.Select(trade => trade.NetProfit));
                var averageLongTrade = AverageTrade(Bot.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit));
                var averageShortTrade = AverageTrade(Bot.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit));
    
                backtestinresult += string.Format("Average Trade,{0},{1},{2}\n", averageTrade, averageLongTrade, averageShortTrade);
    
                var sharpeRatio = SharpeSortino(false, Bot.History.Select(trade => trade.NetProfit));
                var sharpeRatioLong = SharpeSortino(false, Bot.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit));
                var sharpeRatioShort = SharpeSortino(false, Bot.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit));
    
                backtestinresult += string.Format("Sharpe Ratio,{0},{1},{2}\n", sharpeRatio, sharpeRatioLong, sharpeRatioShort);
    
                var sortinoRatio = SharpeSortino(true, Bot.History.Select(trade => trade.NetProfit));
                var sortinoRatioLong = SharpeSortino(true, Bot.History.Where(x => x.TradeType == TradeType.Buy).Select(trade => trade.NetProfit));
                var sortinoRatioShort = SharpeSortino(true, Bot.History.Where(x => x.TradeType == TradeType.Sell).Select(trade => trade.NetProfit));
    
                backtestinresult += string.Format("Sortino Ratio,{0},{1},{2}\n", sortinoRatio, sortinoRatioLong, sortinoRatioShort);
    
    
                File.AppendAllText(FullPath, backtestinresult + Environment.NewLine, Encoding.UTF8);
    
            }
            public void WriteOptimizationResult()
            {
    
                EquityResult = Bot.Account.Equity;
    
                BalanceResult = Bot.Account.Balance;
    
                NetProfitResult = GetNetProfit(Bot.History.Select(trade => trade.NetProfit));
    
                TradesResult = GetTrades(Bot.History.Select(trade => trade.NetProfit));
    
                WinningTradesResult = GetWiningTrades(Bot.History.Select(trade => trade.NetProfit));
    
                LosingTradesResult = GetLosingTrades(Bot.History.Select(trade => trade.NetProfit));
    
                ProfitFactorResult = GetProfitFactor(Bot.History.Select(trade => trade.NetProfit));
    
                //var MaxBalanceDrawDownPct = MaxBalanceDrawDown;
    
                //var MaxEquityDrawDownPct = MaxEquityDrawDown;
    
                SharpeRatioResult = SharpeSortino(false, Bot.History.Select(trade => trade.NetProfit));
    
                SortinoRatioResult = SharpeSortino(true, Bot.History.Select(trade => trade.NetProfit));
    
                AverageTradeResult = AverageTrade(Bot.History.Select(trade => trade.NetProfit));
    
                string data = string.Format("Pass {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}",
                OptimizationPassId, SymbolTimeFrame, EquityResult, BalanceResult, NetProfitResult, TradesResult, WinningTradesResult, LosingTradesResult,
                ProfitFactorResult, MaxEquityDrawDownPct, MaxBalanceDrawDownPct, MaxEquityDrawDown, MaxBalanceDrawDown, SharpeRatioResult, SortinoRatioResult, AverageTradeResult);
    
                bool isEmpty = !parametersValue.Any();
                if (!isEmpty)
                {
                    var result = "," + string.Join(",", parametersValue.Select(s => $"{s}"));
    
                    data += result;
                }
    
                EditLine(data, FullPath, OptimizationPassId + 1);
            }
    
    
            /*public void GetTradeDrawDownData()
            {
                GetMaxBalanceDrawDown(true);
    
                GetMaxBalanceDrawDown(false);
    
                GetMaxEquityDrawDown(true);
    
                GetMaxEquityDrawDown(false);
            }*/
    
            public double GetNetProfit(IEnumerable<double> vals)
            {
                var netprofit = vals.Where(x => x >= 0).Sum();
    
                var netloss = vals.Where(x => x < 0).Sum();
    
                var profit = (netprofit - (netloss * -1));
    
                return (Math.Round(profit, 2));
            }
    
    
            public int GetTrades(IEnumerable<double> vals)
            {
                var totaltrades = vals.Count();
    
                return totaltrades;
            }
    
            public int GetWiningTrades(IEnumerable<double> vals)
            {
                var positiveTrades = vals.Where(x => x >= 0).Count();
    
                return positiveTrades;
            }
            public int GetLosingTrades(IEnumerable<double> vals)
            {
                var negativeTrades = vals.Where(x => x < 0).Count();
    
                return negativeTrades;
            }
    
            public double GetProfitFactor(IEnumerable<double> vals)
            {
                var netprofit = vals.Where(trade => trade >= 0).Sum();
    
                var netloss = vals.Where(x => x < 0).Sum();
    
                var profitFactor = Math.Abs((netprofit / netloss));
    
                return (Math.Round(profitFactor, 2));
            }
            
            
            
            public double GetMaxBalanceDrawDown2(Robot bot, IEnumerable<double> vals,bool isPct)
            {
                double maxBalanceDrawDown = 0.0;
                double startBalance = bot.Account.Balance;
                double minimoValueBalance = vals.Min();                
                if (isPct){
                    maxBalanceDrawDown = MaxEquityBalanceDrawDownToPct(startBalance,minimoValueBalance);
                    //Print("maxBalanceDrawDown = ");
                }else{
                    maxBalanceDrawDown = MaxEquityBalanceDrawDown(startBalance,minimoValueBalance);
                }
                
                return (Math.Round(maxBalanceDrawDown, 2));
            }
            
            
            public double GetMaxEquityDrawDown2(Robot bot, IEnumerable<double> vals,bool isPct)
            {
                double maxEquityDrawDown = 0.0;
                double startEquity = bot.Account.Equity;
                double minimoValueEquity = vals.Min();                
                if (isPct){
                    maxEquityDrawDown = MaxEquityBalanceDrawDownToPct(startEquity,minimoValueEquity);
                    //Print("maxBalanceDrawDown = ");
                }else{
                    maxEquityDrawDown = MaxEquityBalanceDrawDown(startEquity,minimoValueEquity);
                }
                
                return (Math.Round(maxEquityDrawDown, 2));
            }
            
            public double GetMaxBalanceDrawDown(Robot bot, bool isPct)
            {
                double maxBalanceDrawDown = 0.0; // variabile utilizzato sia per il drawdown di balance assoluto che in percentuale
                var Balance = bot.Account.Balance;
                //Print("MaxBalanceDrawDown Balance = "+Balance+" MaxBalance = "+MaxBalance);
    
                if (Balance >= MaxBalance)
                    MaxBalance = Balance;
    
                if (Balance < MaxBalance)
                {
                    var NewLowBalance = Balance;
                    if (isPct)
                    {
                        var CurrentDrawDown = MaxEquityBalanceDrawDownToPct(MaxBalance, NewLowBalance);
                        if (maxBalanceDrawDown < CurrentDrawDown)
                        {
                            maxBalanceDrawDown = Math.Round(CurrentDrawDown, 2);
                        }
                    }
                    else
                    {
                        var CurrentDrawDown = MaxEquityBalanceDrawDown(MaxBalance, NewLowBalance);
                        if (maxBalanceDrawDown < CurrentDrawDown)
                        {
                            maxBalanceDrawDown = Math.Round(CurrentDrawDown, 2);
                        }
                    }
                }
                return maxBalanceDrawDown;
            }
    
            public double GetMaxEquityDrawDown(Robot bot,bool isPct)
            {
                double maxEquityDrawDown = 0.0; // variabile utilizzato sia per il drawdown di equity assoluto che in percentuale
                var Equity = bot.Account.Equity;
    
                if (Equity > MaxEquity)
                    MaxEquity = Equity;
    
                if (Equity < MaxEquity)
                {
                    var NewLowEquity = Equity;
                    if (isPct)
                    {
                        //Print("MaxEquity = "+MaxEquity+" NewLowEquity = "+NewLowEquity);
                        var CurrentDrawDown = MaxEquityBalanceDrawDownToPct(MaxEquity, NewLowEquity);
                        if (maxEquityDrawDown < CurrentDrawDown)
                        {
                            maxEquityDrawDown = Math.Round(CurrentDrawDown, 2);
                        }
                    }
                    else
                    {
                        var CurrentDrawDown = MaxEquityBalanceDrawDown(MaxEquity, NewLowEquity);
                        if (maxEquityDrawDown < CurrentDrawDown)
                        {
                            maxEquityDrawDown = Math.Round(CurrentDrawDown, 2);
                        }
                    }
                }
                return maxEquityDrawDown;
            }
    
            public double MaxEquityBalanceDrawDownToPct(double Max, double Min)
            {
                var difference = Max - Min;
    
                var pct = difference / Max * 100;
    
                return pct;
            }
    
            public double MaxEquityBalanceDrawDown(double Max, double Min)
            {
                var difference = Max - Min;
    
                return difference;
            }
    
            public double AverageTrade(IEnumerable<double> vals)
            {
                double totaltrades = 0.0;
                double netprofit = 0.0;
                try{
                    totaltrades = vals.Count();
                }catch (Exception ex)
                {
                    return 0;
                }
                try{
                    netprofit = vals.Sum();
                }catch (Exception ex)
                {
                    return 0;
                }
    
                return Math.Round((netprofit / totaltrades), 2);
            }
    
    
            public static double SharpeSortino(bool isSortino, IEnumerable<double> vals)
            {
                if (vals.Count() < 2) return double.NaN;
                double average = vals.Average();
                double sd = Math.Sqrt(vals.Where(val => (!isSortino || val < average)).Select(val => (val - average) * (val - average)).Sum() / (vals.Count() - 1));
    
                return Math.Round((average / sd), 2);
            }
    
            public double GetCommissions(IEnumerable<double> vals)
            {
                double commissions = 0.0;
                try{
                    commissions = vals.Sum();
                }catch (Exception ex)
                {
                    commissions = 0;
                }    
                return Math.Round((commissions), 2);
            }
            
            public double GetSwaps(IEnumerable<double> vals)
            {
                double swaps = 0.0;
                try{
                    swaps = vals.Sum();
                }catch (Exception ex)
                {
                    swaps = 0;
                }    
                return Math.Round((swaps), 2);
            }
    
            public int MaxConsecutiveWin(Robot bot,string type)
            {
                var maxwinning = 0;
                var wincounter = 0;
                var lossescounter = 0;
                var maxlosses = 0;
    
                if (type == "Long")
                {
                    foreach (HistoricalTrade trade in bot.History)
                    {
                        if (trade.NetProfit >= 0 && trade.TradeType == TradeType.Buy)
                        {
                            if (lossescounter > 0)
                                lossescounter = 0;
    
                            wincounter += 1;
    
                            if (wincounter > maxwinning)
                                maxwinning = wincounter;
                        }
                        else if (trade.NetProfit < 0 && trade.TradeType == TradeType.Buy)
                            wincounter = 0;
                    }
                }
                else if (type == "Short")
                {
                    foreach (HistoricalTrade trade in bot.History)
                    {
                        if (trade.NetProfit >= 0 && trade.TradeType == TradeType.Sell)
                        {
                            if (lossescounter > 0)
                                lossescounter = 0;
    
                            wincounter += 1;
    
                            if (wincounter > maxwinning)
                                maxwinning = wincounter;
                        }
                        else if (trade.NetProfit < 0 && trade.TradeType == TradeType.Sell)
                            wincounter = 0;
                    }
                }
                else
                {
                    foreach (HistoricalTrade trade in bot.History)
                    {
                        if (trade.NetProfit >= 0)
                        {
                            if (lossescounter > 0)
                                lossescounter = 0;
    
                            wincounter += 1;
    
                            if (wincounter > maxwinning)
                                maxwinning = wincounter;
                        }
                        else if (trade.NetProfit < 0)
                        {
                            if (wincounter > 0)
                                wincounter = 0;
    
                            lossescounter += 1;
    
                            if (lossescounter > maxlosses)
                                maxlosses = lossescounter;
                        }
                    }
                }
    
                return maxwinning;
            }
    
            public double LargesWiningTrade(IEnumerable<double> vals)
            {
                double max = 0.0;
                try{
                    max = vals.Where(x => x >= 0).Max();
                }catch (Exception ex)
                {
                    max = 0;
                }
    
                return max;
            }
    
    
            public double LargestLosingTrade(IEnumerable<double> vals)
            {
                double min = 0.0;
                try{
                    min = vals.Where(x => x < 0).Min();
                }catch (Exception ex)
                {
                    min = 0;
                }
                return min;
            }
    
    
            public int MaxConsecutiveLoss(Robot bot,string type)
            {
                var maxwinning = 0;
                var maxlosses = 0;
                var wincounter = 0;
                var lossescounter = 0;
    
                if (type == "Long")
                {
                    foreach (HistoricalTrade trade in bot.History)
                    {
                        if (trade.NetProfit >= 0 && trade.TradeType == TradeType.Buy)
                            lossescounter = 0;
    
                        else if (trade.NetProfit < 0 && trade.TradeType == TradeType.Buy)
                        {
                            if (wincounter > 0)
                                wincounter = 0;
    
                            lossescounter += 1;
    
                            if (lossescounter > maxlosses)
                                maxlosses = lossescounter;
                        }
                    }
                }
                else if (type == "Short")
                {
                    foreach (HistoricalTrade trade in bot.History)
                    {
                        if (trade.NetProfit >= 0 && trade.TradeType == TradeType.Sell)
                            lossescounter = 0;
    
                        else if (trade.NetProfit < 0 && trade.TradeType == TradeType.Sell)
                        {
                            wincounter = 0;
    
                            lossescounter += 1;
    
                            if (lossescounter > maxlosses)
                                maxlosses = lossescounter;
                        }
                    }
                }
                else
                {
                    foreach (HistoricalTrade trade in bot.History)
                    {
                        if (trade.NetProfit >= 0)
                        {
                            if (lossescounter > 0)
                                lossescounter = 0;
    
                            wincounter += 1;
    
                            if (wincounter > maxwinning)
                                maxwinning = wincounter;
                        }
                        else if (trade.NetProfit < 0)
                        {
                            if (wincounter > 0)
                                wincounter = 0;
    
                            lossescounter += 1;
    
                            if (lossescounter > maxlosses)
                                maxlosses = lossescounter;
                        }
                    }
                }
    
                return maxlosses;
            }
            public void SetSymbolTimeFrame()
            {
                var mystring = Bot.TimeFrame.ToString();
                bool isDigitString = mystring.Any(c => char.IsDigit(c));
                var timeframeString = "";
                if (!isDigitString)
                {
                    if (mystring == "Daily")
                        timeframeString = "1 Day";
                    else
                        timeframeString = "1 " + Bot.TimeFrame.ToString();
                }
                else
                {
                    Regex re = new Regex(@"([a-zA-Z]+)(\d+)");
                    Match result = re.Match(mystring);
                    string alphaPart = result.Groups[1].Value;
                    string numberPart = result.Groups[2].Value;
    
                    timeframeString = numberPart + " " + alphaPart + "s";
                }
    
                SymbolTimeFrame = Bot.Symbol.Name + "-" + timeframeString;
    
            }
    
            public void CreateOptimizationDataFile()
            {
                if (TotalLines() == 0)
                {
                    OptimizationPassId = 1;
    
                    File.AppendAllText(FullPath, header + Environment.NewLine, Encoding.UTF8);
    
                    string data = string.Format("Pass {0}", OptimizationPassId);
    
                    File.AppendAllText(FullPath, data + Environment.NewLine, Encoding.UTF8);
    
                }
                else if (TotalLines() == 1)
                {
                    var Line1 = File.ReadLines(FullPath).First();
                    OptimizationPassId = 1;
    
                    if (Line1 != header)
                    {
                        string data = string.Format("Pass {0}", OptimizationPassId);
    
                        File.AppendAllText(FullPath, data + Environment.NewLine, Encoding.UTF8);
                    }
                    else
                    {
                        string data = string.Format("Pass {0}", OptimizationPassId);
    
                        File.AppendAllText(FullPath, data + Environment.NewLine, Encoding.UTF8);
                    }
                }
                else
                {
                    var lines = TotalLines();
                    OptimizationPassId = lines;
                    string data = string.Format("Pass {0}", OptimizationPassId);
    
                    File.AppendAllText(FullPath, data + Environment.NewLine, Encoding.UTF8);
                }
            }
    
            public void EditLine(string newText, string fileName, int line)
            {
                string[] arrayLines = File.ReadAllLines(fileName);
    
                arrayLines[line - 1] = newText;
    
                File.WriteAllLines(fileName, arrayLines, Encoding.UTF8);
    
            }
    
            public int TotalLines()
            {
                if (!File.Exists(FullPath))
                    return 0;
    
                using (StreamReader r = new StreamReader(FullPath))
                {
                    int i = 0;
                    while (r.ReadLine() != null)
                    {
                        i++;
                    }
                    return i;
                }
            }
        }
        
        
        
        
        
        
        
        



     
    }
}