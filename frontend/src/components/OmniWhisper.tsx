// src/components/OmniWhisper.tsx
import { useState, useEffect, useRef } from 'react';
import { X, Pause, Play, RotateCcw, CornerDownLeft } from 'lucide-react';

// Props per la comunicazione con il componente padre (es. StrategieView)
interface OmniWhisperProps {
  isOpen: boolean;
  onClose: () => void;
  onTextInject: (text: string) => void;
}

export default function OmniWhisper({ isOpen, onClose, onTextInject }: OmniWhisperProps) {
  const [text, setText] = useState('');
  const [isPaused, setIsPaused] = useState(false);
  const [seconds, setSeconds] = useState(0);
  
  // Ref per l'istanza del riconoscimento vocale (Web Speech API)
  const recognitionRef = useRef<any>(null);
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);

  // Gestione del Timer basata sullo stato di attivazione/pausa
  useEffect(() => {
    if (isOpen && !isPaused) {
      timerRef.current = setInterval(() => {
        setSeconds((prev) => prev + 1);
      }, 1000);
    } else {
      if (timerRef.current) clearInterval(timerRef.current);
    }
    return () => {
      if (timerRef.current) clearInterval(timerRef.current);
    };
  }, [isOpen, isPaused]);

  // Effetto per attivare/disattivare il microfono nativo del browser
  useEffect(() => {
    if (!isOpen) return;

    const SpeechRecognition = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;
    if (!SpeechRecognition) {
      console.warn("Riconoscimento vocale non supportato in questo browser.");
      return;
    }

    const rec = new SpeechRecognition();
    rec.lang = 'it-IT';
    rec.continuous = true;
    rec.interimResults = true;

    rec.onresult = (event: any) => {
      let currentTranscript = '';
      for (let i = event.resultIndex; i < event.results.length; ++i) {
        if (event.results[i].isFinal) {
          currentTranscript += event.results[i][0].transcript + ' ';
        }
      }
      if (currentTranscript) {
        setText((prev) => prev + currentTranscript);
      }
    };

    rec.start();
    recognitionRef.current = rec;

    return () => {
      if (recognitionRef.current) {
        recognitionRef.current.stop();
      }
    };
  }, [isOpen]);

  // Gestione Pausa / Riprendi
  const togglePause = () => {
    if (!recognitionRef.current) return;
    if (isPaused) {
      recognitionRef.current.start();
      setIsPaused(false);
    } else {
      recognitionRef.current.stop();
      setIsPaused(true);
    }
  };

  // Reset del buffer di testo e del cronometro
  const handleReset = () => {
    setText('');
    setSeconds(0);
  };

  // Invio definitivo del testo accumulato al modulo chiamante
  const handleConfirm = () => {
    if (text.trim()) {
      onTextInject(text.trim());
    }
    handleReset();
    onClose();
  };

  // Formattazione del tempo in formato mm:ss
  const formatTime = (totalSeconds: number) => {
    const m = Math.floor(totalSeconds / 60).toString().padStart(2, '0');
    const s = (totalSeconds % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
  };

  if (!isOpen) return null;

  return (
    <div className="absolute bottom-20 right-6 w-96 bg-[#020617]/70 backdrop-blur-xl border border-emerald-500/20 rounded-2xl shadow-[0_10px_30px_rgba(0,0,0,0.5)] z-50 overflow-hidden flex flex-col font-mono">
      
      {/* HEADER (Asimmetrico basato su image_8e89bc.png) */}
      <div className="bg-[#0f172a]/40 px-4 py-3 border-b border-emerald-500/10 flex justify-between items-center">
        {/* Nome del widget a sinistra */}
        <span className="text-[11px] font-black text-slate-200 tracking-wider">OMNIWHISPER</span>
        
        {/* Core animato con onde sfumate al centro */}
        <div className="flex items-center space-x-1">
          <div className="h-3 w-0.5 bg-emerald-500/20 rounded-full animate-pulse" />
          <div className="h-4 w-0.5 bg-emerald-500/60 rounded-full animate-pulse delay-75" />
          <div className="h-5 w-0.5 bg-emerald-500 rounded-full animate-pulse delay-150" />
          
          {/* Logo Centrale */}
          <div className={`h-6 w-6 rounded-full border-2 border-emerald-500 bg-[#020617] flex items-center justify-center ${isPaused ? '' : 'animate-pulse'}`}>
            <div className="h-2 w-2 rounded-full bg-emerald-400" />
          </div>
          
          <div className="h-5 w-0.5 bg-emerald-500 rounded-full animate-pulse delay-150" />
          <div className="h-4 w-0.5 bg-emerald-500/60 rounded-full animate-pulse delay-75" />
          <div className="h-3 w-0.5 bg-emerald-500/20 rounded-full animate-pulse" />
        </div>

        {/* Tasto Chiudi a destra */}
        <button onClick={onClose} className="text-slate-400 hover:text-white transition-colors">
          <X size={14} />
        </button>
      </div>

      {/* BODY (Area Trascrizione) */}
      <div className="p-4 bg-[#000000]/30 min-h-[95px] max-h-[140px] overflow-y-auto text-left">
        <p className="text-xs text-slate-300 leading-relaxed">
          {text || <span className="text-slate-600 italic">Sto ascoltando..</span>}
        </p>
      </div>

      {/* FOOTER (Controlli ed Ergonomia) */}
      <div className="px-4 py-3 flex justify-between items-center">
        {/* Timer e pulsanti rapidi sulla sinistra */}
        <div className="flex items-center space-x-3 text-slate-400">
          <span className="text-xs font-bold w-8">{formatTime(seconds)}</span>
          
          {/* Pulsante Pausa */}
          <button 
            onClick={togglePause} 
            className="p-1 rounded border border-emerald-500/20 text-emerald-400 hover:bg-emerald-500/10 transition-colors"
          >
            {isPaused ? <Play size={12} /> : <Pause size={12} />}
          </button>
          
          {/* Pulsante Reset */}
          <button 
            onClick={handleReset} 
            className="p-1 rounded border border-emerald-500/20 text-emerald-400 hover:bg-emerald-500/10 transition-colors"
          >
            <RotateCcw size={12} />
          </button>
        </div>

        {/* Pulsante di inserimento primario */}
        <button 
          onClick={handleConfirm}
          className="flex items-center space-x-1.5 px-4 py-1.5 bg-gradient-to-r from-emerald-500 to-teal-600 text-slate-950 font-bold rounded-lg text-xs tracking-wider shadow-[0_4px_12px_rgba(16,185,129,0.2)] hover:brightness-110 transition-all"
        >
          <CornerDownLeft size={11} strokeWidth={2.5} />
          <span>CONFERMA</span>
        </button>
      </div>

    </div>
  );
}