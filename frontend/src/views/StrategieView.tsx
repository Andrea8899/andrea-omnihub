// src/views/StrategieView.tsx
import { useState, useEffect, useRef } from 'react';
import { 
  Plus, Search, Code, FileText, Edit3, Trash2, 
  ArrowUpDown, Maximize2, X, Mic, Upload, Check, Lock, Unlock, RotateCcw, AlertTriangle, FileCode, Copy,
  Database,
  Terminal,
  Cpu
} from 'lucide-react';
import OmniWhisper from '../components/OmniWhisper';

// ICONA CTRADER PERSONALIZZATA DA SVG UTENTE
const CtraderIcon = ({ className = "h-8 w-8" }: { className?: string }) => (
  <svg viewBox="0 0 400 400" fill="none" xmlns="http://www.w3.org/2000/svg" className={className}>
    <path d="M200 80C133.824 80 80 133.824 80 200C80 266.176 133.824 320 200 320C266.176 320 320 266.176 320 200C320 133.824 266.176 80 200 80ZM200 99.2882C253.771 99.2882 297.835 141.641 300.571 194.741C300.659 196.488 300.712 198.235 300.712 200C300.712 251.547 261.006 270.641 239.741 273.376C206.106 277.7 177.782 259.541 177.782 224C177.782 190.312 215.035 161.141 262.224 175.506L268.329 148.029C251.229 139.718 232.841 135.941 214.876 135.941C145.806 135.941 102.606 187.312 106.594 233.953C106.224 234.147 105.712 234.429 105.2 234.712C102.041 224.282 100.347 212.688 100.347 200C100.347 144.465 144.465 99.2882 200 99.2882Z" fill="currentColor"/>
  </svg>
);

// ICONA PYTHON PERSONALIZZATA DA SVG UTENTE
const PythonIcon = ({ className = "h-8 w-8" }: { className?: string }) => (
  <svg viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" className={className}>
    <title>Python</title>
    <path fill="currentColor" d="M14.25.18l.9.2.73.26.59.3.45.32.34.34.25.34.16.33.1.3.04.26.02.2-.01.13V8.5l-.05.63-.13.55-.21.46-.26.38-.3.31-.33.25-.35.19-.35.14-.33.1-.3.07-.26.04-.21.02H8.77l-.69.05-.59.14-.5.22-.41.27-.33.32-.27.35-.2.36-.15.37-.1.35-.07.32-.04.27-.02.21v3.06H3.17l-.21-.03-.28-.07-.32-.12-.35-.18-.36-.26-.36-.36-.35-.46-.32-.59-.28-.73-.21-.88-.14-1.05-.05-1.23.06-1.22.16-1.04.24-.87.32-.71.36-.57.4-.44.42-.33.42-.24.4-.16.36-.1.32-.05.24-.01h.16l.06.01h8.16v-.83H6.18l-.01-2.75-.02-.37.05-.34.11-.31.17-.28.25-.26.31-.23.38-.2.44-.18.51-.15.58-.12.64-.1.71-.06.77-.04.84-.02 1.27.05zm-6.3 1.98l-.23.33-.08.41.08.41.23.34.33.22.41.09.41-.09.33-.22.23-.34.08-.41-.08-.41-.23-.33-.33-.22-.41-.09-.41.09zm13.09 3.95l.28.06.32.12.35.18.36.27.36.35.35.47.32.59.28.73.21.88.14 1.04.05 1.23-.06 1.23-.16 1.04-.24.86-.32.71-.36.57-.4.45-.42.33-.42.24-.4.16-.36.09-.32.05-.24.02-.16-.01h-8.22v.82h5.84l.01 2.76.02.36-.05.34-.11.31-.17.29-.25.25-.31.24-.38.2-.44.17-.51.15-.58.13-.64.09-.71.07-.77.04-.84.01-1.27-.04-1.07-.14-.9-.2-.73-.25-.59-.3-.45-.33-.34-.34-.25-.34-.16-.33-.1-.3-.04-.25-.02-.2.01-.13v-5.34l.05-.64.13-.54.21-.46.26-.38.3-.32.33-.24.35-.2.35-.14.33-.1.3-.06.26-.04.21-.02.13-.01h5.84l.69-.05.59-.14.5-.21.41-.28.33-.32.27-.35.2-.36.15-.36.1-.35.07-.32.04-.28.02-.21V6.07h2.09l.14.01zm-6.47 14.25l-.23.33-.08.41.08.41.23.33.33.23.41.08.41-.08.33-.23.23-.33.08-.41-.08-.41-.23-.33-.33-.23-.41-.08-.41.08z"/>
  </svg>
);

// ICONA GITHUB 
const GithubIcon = ({ className = "h-8 w-8" }: { className?: string }) => (
  <svg viewBox="0 0 24 24" className={`fill-current ${className}`}>
    <path d="M12 2C6.477 2 2 6.477 2 12c0 4.42 2.865 8.166 6.839 9.489.5.092.682-.217.682-.482 0-.237-.008-.866-.013-1.7-2.782.603-3.369-1.34-3.369-1.34-.454-1.156-1.11-1.462-1.11-1.462-.908-.62.069-.608.069-.608 1.003.07 1.531 1.03 1.531 1.03.892 1.529 2.341 1.087 2.91.831.092-.646.35-1.086.636-1.336-2.22-.253-4.555-1.11-4.555-4.943 0-1.091.39-1.984 1.029-2.683-.103-.253-.446-1.27.098-2.647 0 0 .84-.269 2.75 1.024A9.564 9.564 0 0112 6.844c.85.004 1.705.115 2.504.337 1.909-1.293 2.747-1.024 2.747-1.024.546 1.377.203 2.394.1 2.647.64.699 1.028 1.592 1.028 2.683 0 3.842-2.339 4.687-4.566 4.935.359.309.678.919.678 1.852 0 1.336-.012 2.415-.012 2.743 0 .267.18.579.688.481C19.137 20.162 22 16.418 22 12c0-5.523-4.477-10-10-10z" />
  </svg>
);

// ICONA GEMINI PERSONALIZZATA DA SVG UTENTE
const GeminiIcon = ({ className = "h-4 w-4" }: { className?: string }) => (
  <svg viewBox="0 0 24 24" fill="currentColor" className={className}>
    <path d="M12 2c-.3 0-.5.2-.6.4L9.1 8.1 3.4 10.4c-.3.1-.4.4-.4.6 0 .3.1.5.4.6l5.7 2.3 2.3 5.7c.1.3.3.4.6.4.3 0 .5-.1.6-.4l2.3-5.7 5.7-2.3c.3-.1.4-.3.4-.6 0-.3-.1-.5-.4-.6l-5.7-2.3-2.3-5.7c-.1-.2-.3-.4-.6-.4zm0 3.8l1.4 3.6c.1.2.3.4.5.5l3.6 1.4-3.6 1.4c-.2.1-.4.3-.5.5L12 16.8l-1.4-3.6c-.1-.2-.3-.4-.5-.5L6.5 11.3l3.6-1.4c.2-.1.4-.3.5-.5L12 5.8z" />
  </svg>
);

type SortOption = 'alpha-asc' | 'alpha-desc' | 'date-desc' | 'date-asc';

interface Toast {
  id: string;
  type: 'success' | 'error' | 'warning';
  message: string;
}

interface Strategy {
  id: string;
  title: string;
  created: string;
  createdDisplay: string;
  modified: string;
  isSavedDB: boolean;
  isSavedGit: boolean;
  code: string;
  blocks: {
    entrata: string;
    uscita: string;
    stopLoss: string;
    lottaggio: string;
    parzializzazione: string;
    trailingStop: string;
  };
}

export default function StrategieView() {
  const API_URL = "http://127.0.0.1:8000/api/strategies";

  const [strategieData, setStrategieData] = useState<Strategy[]>([]);
  const [selectedStrategyId, setSelectedStrategyId] = useState<string>('');
  const [viewMode, setViewMode] = useState<'code' | 'desc'>('code');
  const [searchQuery, setSearchQuery] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  
  const [activeModalTab, setActiveModalTab] = useState<'cbot' | 'python'>('cbot');
  
  const [isEditing, setIsEditing] = useState(false);
  const [backupBlocks, setBackupBlocks] = useState<any>(null);
  const [backupCode, setBackupCode] = useState<string>('');
  
  const [sortOption, setSortOption] = useState<SortOption>('date-desc');
  const [isSortMenuOpen, setIsSortMenuOpen] = useState(false);
  const [expandedBlock, setExpandedBlock] = useState<{ label: string; value: string; isModalTarget: boolean; fieldKey?: string } | null>(null);

  const [isCodeZoomOpen, setIsCodeZoomOpen] = useState(false);

  const [isEditingTitle, setIsEditingTitle] = useState(false);
  const [tempTitle, setTempTitle] = useState('');

  const [dbConnected, setDbConnected] = useState<boolean>(false);
  const [gitConnected, setGitConnected] = useState<boolean>(true);

  const [filterLang, setFilterLang] = useState<'all' | 'cbot' | 'python'>('all');

  

  const [toasts, setToasts] = useState<Toast[]>([]);
  const showToast = (type: 'success' | 'error' | 'warning', message: string) => {
    const id = Date.now().toString();
    setToasts(prev => [...prev, { id, type, message }]);
    setTimeout(() => {
      setToasts(prev => prev.filter(t => t.id !== id));
    }, 4000);
  };

  const fileInputRefNew = useRef<HTMLInputElement>(null);
  const fileInputRefEdit = useRef<HTMLInputElement>(null);
  const [isDragging, setIsDragging] = useState(false);

  const [whisperOpen, setWhisperOpen] = useState<boolean>(false);
  const [whisperTarget, setWhisperTarget] = useState<{
    fieldKey: 'entrata' | 'uscita' | 'stopLoss' | 'lottaggio' | 'parzializzazione' | 'trailingStop' | 'title';
    context: 'view' | 'modal' | 'expanded';
  } | null>(null);

  const [newTitle, setNewTitle] = useState('');
  const [newCode, setNewCode] = useState('// Incolla qui o trascina il tuo file di codice cAlgo (.cs, .py, .txt)');
  const [newFileName, setNewFileName] = useState('');
  const [newBlocks, setNewBlocks] = useState({
    entrata: '', uscita: '', stopLoss: '', lottaggio: '', parzializzazione: '', trailingStop: ''
  });

  useEffect(() => {
    checkHealth();
    loadStrategies();
    const interval = setInterval(checkHealth, 10000);
    return () => clearInterval(interval);
  }, []);

  useEffect(() => {
    if (strategieData.length === 0) return;

    const checkGitStatuses = async () => {
      const updatedStrategies = await Promise.all(
        strategieData.map(async (strat) => {
          try {
            const res = await fetch(`http://127.0.0.1:8000/api/strategies/status/${strat.id}`);
            if (res.ok) {
              const data = await res.json();
              return { ...strat, isSavedGit: data.is_on_github };
            }
          } catch (e) {
            console.error("Errore polling status Git", e);
          }
          return strat;
        })
      );

      const isChanged = JSON.stringify(updatedStrategies.map(s => s.isSavedGit)) !== JSON.stringify(strategieData.map(s => s.isSavedGit));
      if (isChanged) {
        setStrategieData(updatedStrategies);
      }
    };

    const gitInterval = setInterval(checkGitStatuses, 4000);
    return () => clearInterval(gitInterval);
  }, [strategieData]);

  const checkHealth = async () => {
    try {
      const res = await fetch("http://127.0.0.1:8000/api/health");
      if (res.ok) {
        const data = await res.json();
        setDbConnected(data.status === "connected");
      } else {
        setDbConnected(false);
      }
    } catch {
      setDbConnected(false);
    }
  };

  const loadStrategies = async () => {
    try {
      const response = await fetch(API_URL);
      if (response.ok) {
        const data = await response.json();
        const mappedData = data.map((item: any) => ({
          ...item,
          isSavedDB: true,
          isSavedGit: item.isSavedGit ?? false
        }));
        setStrategieData(mappedData);
        if (mappedData.length > 0 && !selectedStrategyId) {
          setSelectedStrategyId(mappedData[0].id);
        }
      }
    } catch {
      showToast('error', 'Impossibile connettersi al server di Backend Python.');
    }
  };

  const clientValidate = (title: string, blocks: any) => {
    if (!title.trim() || /^\d+$/.test(title.trim())) {
      showToast('error', 'Il nome della strategia non può essere vuoto o contenere solo numeri.');
      return false;
    }
    for (const key in blocks) {
      const val = blocks[key]?.trim() || '';
      if (val && /^\d+$/.test(val)) {
        showToast('error', `Il blocco ${key.toUpperCase()} non può essere composto da soli numeri.`);
        return false;
      }
    }
    return true;
  };

  const handleFileContentLoad = (file: File, target: 'new' | 'edit') => {
    const validExtensions = ['.cs', '.py', '.txt'];
    const fileExt = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
    
    if (!validExtensions.includes(fileExt)) {
      showToast('warning', 'Formato file non supportato. Accettati solo: .cs, .py, .txt');
      return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      const content = e.target?.result as string;
      if (target === 'new') {
        setNewCode(content);
        setNewFileName(file.name);
        showToast('success', `File ${file.name} caricato correttamente.`);
      } else {
        setStrategieData(prev => prev.map(s => s.id === selectedStrategyId ? { ...s, code: content } : s));
        showToast('success', `Nuovo codice da ${file.name} importato con successo.`);
      }
    };
    reader.readAsText(file);
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = () => {
    setIsDragging(false);
  };

  const handleDrop = (e: React.DragEvent, target: 'new' | 'edit') => {
    e.preventDefault();
    setIsDragging(false);
    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      handleFileContentLoad(e.dataTransfer.files[0], target);
    }
  };

  const handleStartEditing = () => {
    const current = strategieData.find(s => s.id === selectedStrategyId);
    if (current) {
      setBackupBlocks({ ...current.blocks });
      setBackupCode(current.code);
    }
    setIsEditing(true);
    showToast('warning', 'Sei entrato in modalità modifica.');
  };

  const handleConfirmEditing = async () => {
    const current = strategieData.find(s => s.id === selectedStrategyId);
    if (!current) return;
    if (!clientValidate(current.title, current.blocks)) return;

    try {
      const response = await fetch(`${API_URL}/${selectedStrategyId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(current)
      });
      if (response.ok) {
        setIsEditing(false);
        setBackupBlocks(null);
        showToast('success', 'Modifiche salvate! Sincronizzazione GitHub avviata in background.');
        loadStrategies();
      } else {
        const err = await response.json();
        showToast('error', err.detail || 'Errore durante l\'aggiornamento.');
      }
    } catch {
      showToast('error', 'Errore di rete con il server.');
    }
  };

  const handleCancelEditing = () => {
    if (backupBlocks) {
      setStrategieData(prev => prev.map(s => s.id === selectedStrategyId ? { ...s, blocks: { ...backupBlocks }, code: backupCode } : s));
    }
    setIsEditing(false);
    setBackupBlocks(null);
    showToast('warning', 'Modifiche annullate.');
  };

  const handleWhisperInject = (textToInject: string) => {
    if (!whisperTarget) return;
    const { fieldKey, context } = whisperTarget;
    const cleanText = textToInject.trim();
    const appendText = (cur: string) => cur ? `${cur.trim()} ${cleanText}` : cleanText;

    if (fieldKey === 'title') {
      setNewTitle(prev => appendText(prev));
      return;
    }

    if (context === 'modal') {
      setNewBlocks(prev => ({ ...prev, [fieldKey]: appendText(prev[fieldKey as keyof typeof prev]) }));
    } else {
      setStrategieData(prevData => prevData.map(strat => {
        if (strat.id === selectedStrategyId) {
          return { ...strat, blocks: { ...strat.blocks, [fieldKey]: appendText(strat.blocks[fieldKey as keyof typeof strat.blocks]) } };
        }
        return strat;
      }));
      if (context === 'expanded' && expandedBlock) {
        setExpandedBlock(prev => prev ? { ...prev, value: appendText(prev.value) } : null);
      }
    }
    showToast('success', 'Testo trascritto da OmniWhisper inserito.');
  };

  const handleSaveStrategy = async () => {
    if (!clientValidate(newTitle, newBlocks)) return;
    
    const nameExists = strategieData.some(s => s.title.toLowerCase().trim() === newTitle.trim().toLowerCase());
    if (nameExists) {
      showToast('error', `Esiste già una strategia chiamata "${newTitle.trim()}". Scegli un altro nome.`);
      return;
    }

    const newStrategyData = {
      id: Date.now().toString(),
      title: newTitle,
      code: newCode,
      blocks: newBlocks
    };

    try {
      const response = await fetch(API_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(newStrategyData)
      });

      if (response.ok) {
        setIsModalOpen(false);
        setNewTitle('');
        setNewCode('// Incolla qui o trascina il tuo file di codice cAlgo (.cs, .py, .txt)');
        setNewFileName('');
        setNewBlocks({ entrata: '', uscita: '', stopLoss: '', lottaggio: '', parzializzazione: '', trailingStop: '' });
        setSelectedStrategyId(newStrategyData.id);
        showToast('success', 'Nuova strategia creata! Sincronizzazione GitHub avviata in background.');
        loadStrategies();
      } else {
        const err = await response.json();
        showToast('error', err.detail || 'Errore nel salvataggio.');
      }
    } catch {
      showToast('error', 'Errore di connessione al database.');
    }
  };

  const handleDeleteStrategy = async () => {
    if (!selectedStrategyId) return;
    if (!window.confirm("Sei sicuro di voler eliminare definitivamente questa strategia dal database locale?")) return;

    try {
      const response = await fetch(`${API_URL}/${selectedStrategyId}`, { method: 'DELETE' });
      if (response.ok) {
        setSelectedStrategyId('');
        showToast('success', 'Eliminazione avviata! Rimozione da GitHub in corso.');
        loadStrategies();
      }
    } catch {
      showToast('error', 'Impossibile eliminare la strategia.');
    }
  };

  const handleCopyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
    showToast('success', 'Codice sorgente copiato negli appunti con successo!');
  };

  const renderHighlightedCode = (codeText: string) => {
    if (!codeText) return null;
    return codeText.split('\n').map((line, i) => {
      const tokens = line.split(/(\s+|,|\(|\)|\{|\}|\[|\]|;|\.)/);
      return (
        <div key={i} className="min-h-[1.2rem]">
          {tokens.map((token, index) => {
            if (['using', 'namespace', 'public', 'class', 'protected', 'override', 'void', 'string', 'int', 'double', 'import', 'def', 'return'].includes(token)) {
              return <span key={index} className="text-[#569cd6] font-bold">{token}</span>;
            }
            if (['cAlgo', 'API', 'Indicators', 'Robot', 'Print'].includes(token)) {
              return <span key={index} className="text-[#4ec9b0]">{token}</span>;
            }
            if (token.startsWith('//') || token.startsWith('#')) {
              return <span key={index} className="text-[#6a9955] italic">{token}</span>;
            }
            if (token.startsWith('"') && token.endsWith('"')) {
              return <span key={index} className="text-[#ce9178]">{token}</span>;
            }
            return <span key={index}>{token}</span>;
          })}
        </div>
      );
    });
  };

  const filteredAndSortedStrategies = strategieData
    .filter(s => {
    // 1. Verifica se corrisponde alla ricerca testuale
    const matchesSearch = s.title.toLowerCase().includes(searchQuery.toLowerCase());
    if (!matchesSearch) return false;

    // 2. Determina se la strategia è Python (cerca la parola "def" nel codice o "py" nel titolo)
    const isStratPython = s.code?.includes('def ') || s.title.toLowerCase().includes('py');
    
    // 3. Applica il filtro del selettore
    if (filterLang === 'python') return isStratPython;
    if (filterLang === 'cbot') return !isStratPython;
    return true; // Se è su 'all' mostra tutto
    })
    .sort((a, b) => {
      if (sortOption === 'alpha-asc') return a.title.localeCompare(b.title);
      if (sortOption === 'alpha-desc') return b.title.localeCompare(a.title);
      if (sortOption === 'date-desc') return new Date(b.created || b.id).getTime() - new Date(a.created || a.id).getTime();
      if (sortOption === 'date-asc') return new Date(a.created || a.id).getTime() - new Date(b.created || b.id).getTime();
      return 0;
    });

  const currentStrategy = strategieData.find(s => s.id === selectedStrategyId) || strategieData[0];

  // Riconoscimento della tecnologia (per mostrare dinamicamente l'icona adatta)
  const isPythonStrategy = currentStrategy?.code?.includes('def ') || currentStrategy?.title?.toLowerCase().includes('py');

  return (
    <div className="flex-1 flex flex-col space-y-4 min-h-0 relative text-left -mt-4">
      
      {/* TOAST SYSTEM CONTAINER */}
      <div className="fixed top-4 right-4 z-50 flex flex-col space-y-2 pointer-events-none max-w-sm w-full">
        {toasts.map(t => (
          <div key={t.id} className={`p-4 rounded-xl border shadow-2xl flex items-center space-x-3 pointer-events-auto transition-all animate-slide-in ${
            t.type === 'success' ? 'bg-[#0a1612] border-emerald-500/40 text-emerald-400' :
            t.type === 'error' ? 'bg-[#1a0f12] border-rose-500/40 text-rose-400' :
            'bg-[#19140f] border-amber-500/40 text-amber-400'
          }`}>
            {t.type === 'error' ? <AlertTriangle size={18} /> : <Check size={18} />}
            <span className="text-xs font-mono font-bold tracking-wide flex-1">{t.message}</span>
          </div>
        ))}
      </div>

      {/* SIDEBAR STATUS BAR OVERLAY INTEGRATION */}
      <div className="absolute -left-[280px] bottom-4 w-[240px] hidden xl:flex flex-col space-y-2 bg-[#080b16] border border-slate-900 rounded-xl p-3 font-mono text-[11px] tracking-wider">
        <div className="flex items-center justify-between">
          <span className="text-slate-500 flex items-center space-x-1.5">
            <CtraderIcon className="h-3.5 w-3.5 text-slate-500" /> <span>CTRADER TECH</span>
          </span>
          <span className="text-emerald-400 font-bold px-2 py-0.5 rounded text-[10px] bg-emerald-500/10 border border-emerald-500/20">
            PRONTO
          </span>
        </div>
        <div className="flex items-center justify-between">
          <span className="text-slate-500 flex items-center space-x-1.5">
            <PythonIcon className="h-3.5 w-3.5 text-slate-500" /> <span>PYTHON ENGINE</span>
          </span>
          <span className={`font-bold px-2 py-0.5 rounded text-[10px] ${gitConnected ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20' : 'bg-slate-800 text-slate-500 border border-slate-700'}`}>
            {gitConnected ? 'CONNESSO' : 'DISCONNESSO'}
          </span>
        </div>
      </div>

      {/* HEADER BAR */}
      <div className="flex justify-between items-center shrink-0 bg-[#0c1020] p-3 rounded-xl border border-slate-900 shadow-lg">
        <button 
          onClick={() => { setActiveModalTab('cbot'); setIsModalOpen(true); }}
          className="flex items-center space-x-2 bg-emerald-500 hover:bg-emerald-400 text-slate-950 px-4 py-2 rounded-lg font-bold text-xs tracking-wider transition-colors shadow-[0_0_15px_rgba(16,185,129,0.2)]"
        >
          <Plus size={16} strokeWidth={3} />
          <span>NUOVA STRATEGIA</span>
        </button>
      </div>

      <div className="text-sm font-bold tracking-wider text-slate-400 uppercase shrink-0">
        Strategie Salvate
      </div>

      {/* LAYOUT GRID */}
      <div className="flex-1 flex space-x-6 min-h-0 pb-1">
        
        {/* SIDEBAR STRATEGIE */}
        <div className="w-1/3 flex flex-col space-y-4 overflow-y-auto pr-1">
          
          <div className="flex space-x-2 shrink-0">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" size={14} />
              <input 
                type="text" 
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Cerca per titolo..." 
                className="w-full bg-[#111830] border border-slate-800 rounded-lg pl-9 pr-4 py-2 text-xs text-slate-300 placeholder-slate-500 focus:outline-none focus:border-emerald-500/50 font-mono transition-colors"
              />
            </div>
            
            
            <div className="relative">
              <button 
                onClick={() => setIsSortMenuOpen(!isSortMenuOpen)}
                className="p-2 bg-[#111830] border border-slate-800 rounded-lg text-slate-400 hover:text-white flex items-center justify-center space-x-1 px-3 text-xs"
              >
                <span>Ordina</span>
                <ArrowUpDown size={12} />
              </button>
              {isSortMenuOpen && (
                <div className="absolute right-0 mt-2 w-56 bg-[#111625] border border-slate-800 rounded-lg shadow-2xl z-50 p-1 font-mono text-[11px]">
                  {[
                    { id: 'date-desc', label: 'Più recente (Modifica)' },
                    { id: 'date-asc', label: 'Meno recente (Modifica)' },
                    { id: 'alpha-asc', label: 'Alfabetico (A-Z)' }, 
                    { id: 'alpha-desc', label: 'Alfabetico (Z-A)' }
                  ].map((opt) => (
                    <button
                      key={opt.id}
                      onClick={() => { setSortOption(opt.id as SortOption); setIsSortMenuOpen(false); }}
                      className="w-full text-left px-3 py-2 text-slate-300 hover:bg-[#1b254b] hover:text-emerald-400 rounded flex items-center justify-between transition-colors"
                    >
                      <span>{opt.label}</span>
                      {sortOption === opt.id && <Check size={12} className="text-emerald-400" />}
                    </button>
                  ))}
                </div>
              )}
            </div>
            
          </div>
          <div className="flex bg-[#0a0d1a] border border-slate-900 rounded-lg p-0.5 shrink-0 font-mono text-[10px] font-bold tracking-wider">
              {(['all', 'cbot', 'python'] as const).map((lang) => (
                <button
                  key={lang}
                  onClick={() => setFilterLang(lang)}
                  className={`flex-1 py-1.5 rounded transition-all uppercase ${
                    filterLang === lang 
                      ? 'bg-[#162244] text-emerald-400 border border-emerald-500/20 shadow-[0_0_10px_rgba(16,185,129,0.05)]' 
                      : 'text-slate-500 hover:text-slate-300'
                  }`}
                >
                  {lang === 'all' ? 'Tutte' : lang === 'cbot' ? 'cBot (C#)' : 'Python'}
                </button>
              ))}
            </div>

          <div className="space-y-3">
            {filteredAndSortedStrategies.map((strat) => {
              const isSelected = strat.id === selectedStrategyId;
              const isStratPython = strat.code?.includes('def ') || strat.title.toLowerCase().includes('py');

              return (
                <div 
                  key={strat.id}
                  onClick={() => { setSelectedStrategyId(strat.id); setIsEditing(false); }}
                  className={`p-5 rounded-xl border transition-all cursor-pointer flex flex-col space-y-4  relative ${
                    isSelected ? 'border-emerald-500 bg-[#162244] shadow-[0_0_15px_rgba(16,185,129,0.1)]' : 'border-slate-800 bg-[#0f152d] hover:border-slate-700 hover:bg-[#141d3d]'
                  }`}
                >
                  <div className="flex items-center justify-between">
                    {/* FONT-SIZE INCREMENTATO (text-lg e font-black) */}
                    <span className="text-lg font-black text-white tracking-wide">{strat.title}</span>
                    <div className="flex items-center space-x-3">
                      {/*{isStratPython ? (
                        <PythonIcon className={`h-6 w-6 transition-colors ${isSelected ? 'text-emerald-400' : 'text-slate-400'}`} />
                      ) : (
                        <CtraderIcon className={`h-6 w-6 transition-colors ${isSelected ? 'text-emerald-400' : 'text-slate-400'}`} />
                      )}*/}
                      <Database className={`h-4 w-4 transition-colors ${strat.isSavedDB && dbConnected ? 'text-emerald-400' : 'text-slate-600'}`} />
                      <GithubIcon className={`h-5 w-5 transition-colors ${strat.isSavedGit && gitConnected ? 'text-emerald-400' : 'text-slate-600'}`} />
                    </div>
                  </div>
                  {/* FONT-SIZE INCREMENTATO NEI DETTAGLI (text-sm) */}
                  <div className="space-y-2 text-[13px] text-slate-300 font-medium tracking-wide">
                    <div>Creata il: <span className="text-white font-bold font-mono bg-slate-900/60 px-2 py-0.5 rounded">{strat.createdDisplay || '...'}</span></div>
                    <div>Modifica il: <span className="text-white font-bold font-mono bg-slate-900/60 px-2 py-0.5 rounded">{strat.modified || '...'}</span></div>
                  </div>
                  
                  {/* FRAMEWORK ICON ASSIGNMENT IN THE BOTTOM RIGHT */}
                  <div className="absolute bottom-3 right-3 text-slate-500/80">
                    {isStratPython ? (
                      
                      /*<div className="flex items-center space-x-1 text-[9px] font-bold font-mono text-amber-500/80 bg-amber-500/5 px-1.5 py-0.5 rounded border border-amber-500/10">*/
                      <PythonIcon className={`h-5 w-5 transition-colors ${isSelected ? 'text-white' : 'text-slate-400'}`} />
                      /*</div>*/
                    ) : (
                      /*<div className="flex items-center space-x-1 text-[9px] font-bold font-mono text-emerald-400/80 bg-emerald-400/15 px-1.5 py-0.5 rounded border border-emerald-500/10">
                      */
                        <CtraderIcon className={`h-8 w-8 transition-colors ${isSelected ? 'text-white' : 'text-slate-400'}`} />
                      /*</div>*/
                    )}
                  </div>
                </div>                
              );
            })}
          </div>
        </div>

        {/* WORKSPACE DETTAGLIO */}
        <div className="flex-1 bg-[#0e1326]/40 border border-slate-800 rounded-xl p-5 flex flex-col justify-between overflow-hidden">
          {currentStrategy ? (
            <div className="flex flex-col space-y-3 min-h-0 flex-1">
              
              <div className="flex items-center justify-between shrink-0">
                <div className="flex items-center space-x-3 w-full justify-between">
                  
                  {isEditingTitle ? (
                    <div className="flex items-center space-x-2">
                      <input
                        type="text"
                        value={tempTitle}
                        onChange={(e) => setTempTitle(e.target.value)}
                        className="bg-slate-900 border border-slate-700 rounded px-2 py-1 text-sm text-white font-bold uppercase focus:outline-none focus:border-emerald-500"
                        autoFocus
                      />
                      <button 
                        onClick={async () => {
                          if (!tempTitle.trim() || /^\d+$/.test(tempTitle.trim())) {
                            return;
                          }

                          const nameExists = strategieData.some(
                            s => s.title.toLowerCase().trim() === tempTitle.trim().toLowerCase() && s.id !== currentStrategy.id
                          );
                          if (nameExists) return;

                          try {
                            const res = await fetch(`http://127.0.0.1:8000/api/strategies/${currentStrategy.id}`, {
                              method: 'PUT',
                              headers: { 'Content-Type': 'application/json' },
                              body: JSON.stringify({ ...currentStrategy, title: tempTitle.trim() })
                            });
                            
                            if (!res.ok) return;
                            
                            setStrategieData(prev => prev.map(s => s.id === currentStrategy.id ? { ...s, title: tempTitle.trim() } : s));
                            setIsEditingTitle(false);
                            showToast('success', 'Titolo aggiornato in background.');
                            loadStrategies();
                          } catch (err) {}
                        }}
                        className="p-1 bg-emerald-500/20 text-emerald-400 border border-emerald-500/30 rounded hover:bg-emerald-500/30 text-xs font-bold"
                      >
                        SALVA
                      </button>
                      <button 
                        onClick={() => setIsEditingTitle(false)}
                        className="p-1 bg-slate-800 text-slate-400 rounded hover:bg-slate-700 text-xs"
                      >
                        ANNULLA
                      </button>
                    </div>
                  ) : (
                    <div className="flex items-center space-x-2">
                      <h3 className="text-xl font-black text-white tracking-wide font-mono uppercase">{currentStrategy.title}</h3>
                      <button 
                        onClick={() => {
                          setTempTitle(currentStrategy.title);
                          setIsEditingTitle(true);
                        }}
                        className="text-slate-500 hover:text-emerald-400 transition-colors p-1"
                        title="Rinomina strategia"
                      >
                        <Edit3 size={16} />
                      </button>
                    </div>
                  )}

                  {/* INDICATORI LUCCICHETTO + ICONA TECNOLOGICA (A DESTRA) */}
                  <div className="flex items-center space-x-2">
                    {isPythonStrategy ? (
                      <span className="flex items-center space-x-1 text-[10px] bg-blue-500/10 text-blue-400 border border-blue-500/20 px-2 py-0.5 rounded font-mono font-bold">
                        <PythonIcon className="h-3 w-3" /> <span>PYTHON SUITE</span>
                      </span>
                    ) : (
                      <span className="flex items-center space-x-1 text-[10px] bg-cyan-500/10 text-cyan-400 border border-cyan-500/20 px-2 py-0.5 rounded font-mono font-bold">
                        <CtraderIcon className="h-3 w-3" /> <span>CTRADER ALGO</span>
                      </span>
                    )}

                    {isEditing ? (
                      <span className="flex items-center space-x-1 text-[10px] bg-amber-500/10 text-amber-400 border border-amber-500/20 px-2 py-0.5 rounded font-bold animate-pulse">
                        <Unlock size={10} /><span>MODALITÀ MODIFICA</span>
                      </span>
                    ) : (
                      <span className="flex items-center space-x-1 text-[10px] bg-slate-800 text-slate-400 border border-slate-700 px-2 py-0.5 rounded font-bold">
                        <Lock size={10} /><span>LETTURA PROTETTA</span>
                      </span>
                    )}
                  </div>

                </div>
              </div>

              <div className="flex space-x-2 shrink-0">
                <button onClick={() => setViewMode('code')} className={`flex items-center space-x-2 px-4 py-2 rounded-lg text-xs font-bold tracking-wide transition-all ${viewMode === 'code' ? 'bg-[#1b254b] border border-emerald-500/30 text-emerald-400' : 'bg-slate-800/40 text-slate-400 hover:text-white'}`}>
                  <Code size={13} /><span>Codice</span>
                </button>
                <button onClick={() => setViewMode('desc')} className={`flex items-center space-x-2 px-4 py-2 rounded-lg text-xs font-bold tracking-wide transition-all ${viewMode === 'desc' ? 'bg-[#1b254b] border border-emerald-500/30 text-emerald-400' : 'bg-slate-800/40 text-slate-400 hover:text-white'}`}>
                  <FileText size={13} /><span>6 Fondamenta</span>
                </button>
              </div>

              <div className="flex-1 min-h-0">
                {viewMode === 'code' ? (
                <div 
                    onDragOver={isEditing ? handleDragOver : undefined}
                    onDragLeave={isEditing ? handleDragLeave : undefined}
                    onDrop={isEditing ? (e) => handleDrop(e, 'edit') : undefined}
                    className={`h-full border rounded-lg relative font-mono text-xs overflow-hidden flex flex-col shadow-2xl transition-all ${
                    isDragging ? 'border-amber-500 bg-amber-500/5 scale-[0.99]' : 'border-slate-900 bg-[#1e1e1e]'
                    }`}
                >
                    <div className="bg-[#181818] border-b border-slate-900 px-4 py-2 flex justify-between items-center shrink-0 select-none">
                    <span className="text-[10px] text-slate-500 font-bold tracking-wider">
                        PREVIEW CODICE (MOSTRATE 15 DI {currentStrategy.code?.split('\n').length || 0} RIGHE)
                    </span>
                    <div className="flex items-center space-x-2">
                        {isEditing && (
                        <>
                            <input type="file" ref={fileInputRefEdit} accept=".cs,.py,.txt" onChange={(e) => e.target.files?.[0] && handleFileContentLoad(e.target.files[0], 'edit')} className="hidden" />
                            <button onClick={() => fileInputRefEdit.current?.click()} className="flex items-center space-x-1 px-2.5 py-1 bg-slate-900/90 border border-slate-700 rounded text-[10px] font-bold text-amber-400 hover:bg-slate-800 transition-colors">
                            <Upload size={11} /> <span>SOSTITUISCI</span>
                            </button>
                        </>
                        )}
                        <button 
                        onClick={() => setIsCodeZoomOpen(true)}
                        className="flex items-center space-x-1 px-2.5 py-1 bg-emerald-500/10 border border-emerald-500/20 rounded text-[10px] font-bold text-emerald-400 hover:bg-emerald-500/20 transition-all"
                        >
                        <Maximize2 size={11} /> <span>VEDI CODICE INTERO</span>
                        </button>
                    </div>
                    </div>

                    {isDragging && (
                    <div className="absolute inset-0 z-20 bg-slate-950/80 flex flex-col items-center justify-center text-amber-400 font-bold space-y-2">
                        <FileCode size={32} />
                        <span>Rilascia qui il file per sovrascrivere il codice</span>
                    </div>
                    )}

                    <div className="flex-1 overflow-auto p-4 max-h-[350px]">
                    <div className="flex space-x-4 min-w-max select-text">
                        <div className="text-[#858585] select-none text-right pr-3 border-r border-[#3c3c3c] space-y-1">
                        {currentStrategy.code?.split('\n').slice(0, 15).map((_, i) => <div key={i}>{i + 1}</div>) || <div>1</div>}
                        </div>
                        <div className="text-[#d4d4d4] space-y-1 tracking-wide">
                        {renderHighlightedCode(currentStrategy.code?.split('\n').slice(0, 15).join('\n'))}
                        
                        {(currentStrategy.code?.split('\n').length || 0) > 15 && (
                            <div className="text-slate-600 font-bold pt-2 select-none italic text-[11px]">
                            ... Altre {(currentStrategy.code?.split('\n').length || 0) - 15} righe nascoste. Clicca su "Vedi Codice Intero" per analizzarle.
                            </div>
                        )}
                        </div>
                    </div>
                    </div>
                </div>
                ) : (
                  <div className="grid grid-cols-2 grid-rows-3 gap-3 h-full pb-1">
                    {[
                      { key: 'entrata', label: 'ENTRATA' }, { key: 'uscita', label: 'USCITA' },
                      { key: 'stopLoss', label: 'STOP LOSS' }, { key: 'lottaggio', label: 'LOTTAGGIO' },
                      { key: 'parzializzazione', label: 'PARZIALIZZAZIONE/AUMENTO' }, { key: 'trailingStop', label: 'TRAILING STOP' }
                    ].map((block) => (
                      <div key={block.key} className={`border rounded-lg p-3 flex flex-col justify-between text-left transition-all relative ${isEditing ? 'border-amber-500/40 bg-[#16141f]' : 'border-slate-800 bg-[#0b0f19]'}`}>
                        <div className="space-y-1 overflow-hidden flex-1 flex flex-col">
                          <div className="flex justify-between items-center shrink-0">
                            <span className="text-[11px] font-bold text-emerald-400 tracking-wider font-mono">{block.label}</span>
                            {isEditing && (
                              <button onClick={() => { setWhisperTarget({ fieldKey: block.key as any, context: 'view' }); setWhisperOpen(true); }} className="flex items-center space-x-1.5 px-2 py-0.5 rounded text-[10px] font-bold transition-all border bg-amber-500/10 text-amber-400 border-amber-500/30 hover:bg-amber-500/35">
                                <Mic size={12} /><span>DETTA</span>
                              </button>
                            )}
                          </div>
                          {isEditing ? (
                            <textarea
                              value={currentStrategy.blocks[block.key as keyof typeof currentStrategy.blocks] || ''}
                              onChange={(e) => {
                                const val = e.target.value;
                                setStrategieData(prev => prev.map(s => s.id === currentStrategy.id ? { ...s, blocks: { ...s.blocks, [block.key]: val } } : s));
                              }}
                              className="w-full bg-[#05070f] text-xs text-slate-200 focus:outline-none resize-none font-mono flex-1 p-1.5 rounded border border-slate-800 focus:border-amber-500"
                            />
                          ) : (
                            <p className="text-xs text-slate-300 font-mono whitespace-pre-line overflow-y-auto flex-1 select-text pr-1">
                              {currentStrategy.blocks[block.key as keyof typeof currentStrategy.blocks] || 'Nessuna specifica.'}
                            </p>
                          )}
                        </div>
                        <div className="flex justify-end pt-1 shrink-0">
                          <button onClick={() => setExpandedBlock({ label: block.label, value: currentStrategy.blocks[block.key as keyof typeof currentStrategy.blocks] || '', isModalTarget: false, fieldKey: block.key })} className="text-slate-500 hover:text-emerald-400 transition-colors flex items-center space-x-1 text-[10px]">
                            <Maximize2 size={11} /><span>Espandi</span>
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          ) : (
            <div className="flex-1 flex flex-col items-center justify-center text-xs text-slate-500 font-mono">
              Seleziona o crea una strategia dal pannello laterale.
            </div>
          )}

          {/* FOOTER DI CONTROLLO SALVATAGGIO */}
          {currentStrategy && (
            <div className="flex justify-end space-x-3 pt-3 shrink-0 border-t border-slate-800/50 mt-2">
              <button 
                onClick={() => isEditing ? handleConfirmEditing() : handleStartEditing()}
                className={`flex items-center space-x-2 px-4 py-2 rounded-lg font-bold text-xs tracking-wider transition-colors border ${
                  isEditing ? 'bg-emerald-500 text-slate-950 border-emerald-400 hover:bg-emerald-400' : 'bg-slate-800 text-slate-200 border-slate-700 hover:bg-slate-700'
                }`}
              >
                <Edit3 size={13} />
                <span>{isEditing ? 'Conferma Modifiche' : 'Modifica'}</span>
              </button>
              {isEditing ? (
                <button onClick={handleCancelEditing} className="flex items-center space-x-2 bg-slate-800 hover:bg-slate-700 text-slate-300 px-4 py-2 rounded-lg font-bold text-xs tracking-wider transition-colors border border-slate-700">
                  <RotateCcw size={13} /><span>Annulla</span>
                </button>
              ) : (
                <button onClick={handleDeleteStrategy} className="flex items-center space-x-2 bg-rose-500/10 hover:bg-rose-500 text-rose-400 hover:text-white px-4 py-2 rounded-lg font-bold text-xs tracking-wider transition-all border border-rose-500/20">
                  <Trash2 size={13} /><span>Elimina</span>
                </button>
              )}
            </div>
          )}
        </div>
      </div>

      {isModalOpen && (
        <div className="fixed inset-0 z-40 flex items-center justify-center bg-slate-950/80 backdrop-blur-sm p-4">
          <div className="bg-[#111625] border border-slate-800 rounded-xl w-full max-w-3xl max-h-[95vh] flex flex-col overflow-hidden shadow-2xl">
            
            {/* MODAL HEADER + TAB SELECTOR */}
            <div className="p-5 border-b border-slate-800/80 flex flex-col space-y-4 shrink-0">
              <div className="flex justify-between items-center">
                <h2 className="text-lg font-bold text-white tracking-wider font-mono">Inizializza Strategia Quantitativa</h2>
                <button onClick={() => setIsModalOpen(false)} className="text-slate-400 hover:text-white transition-colors"><X size={20} /></button>
              </div>
              
              {/* SELETTORE DEI TAB AGGIORNATO */}
              <div className="flex bg-[#070b13] p-1 rounded-xl border border-slate-900 w-full sm:w-max font-mono text-xs">
                <button 
                  type="button"
                  onClick={() => { setActiveModalTab('cbot'); setNewCode('// Incolla qui o trascina il tuo file di codice cAlgo (.cs)'); }}
                  className={`flex items-center space-x-2 px-5 py-2.5 rounded-lg font-bold transition-all ${
                    activeModalTab === 'cbot' 
                      ? 'bg-[#1b254b] text-emerald-400 border border-emerald-500/20 shadow-md' 
                      : 'text-slate-500 hover:text-slate-300'
                  }`}
                >
                  <CtraderIcon className="h-4 w-4" />
                  <span>cBot (cTrader)</span>
                </button>
                <button 
                  type="button"
                  onClick={() => { setActiveModalTab('python'); setNewCode('# Incolla qui o trascina il tuo script Python (.py)'); }}
                  className={`flex items-center space-x-2 px-5 py-2.5 rounded-lg font-bold transition-all ${
                    activeModalTab === 'python' 
                      ? 'bg-[#1b254b] text-emerald-400 border border-emerald-500/20 shadow-md' 
                      : 'text-slate-500 hover:text-slate-300'
                  }`}
                >
                  <PythonIcon className="h-4 w-4" />
                  <span>Python</span>
                </button>
              </div>
            </div>

            {/* CONTENUTO IN BASE AL TAB SELEZIONATO */}
            {activeModalTab === 'cbot' ? (
              <>
                <div className="p-6 overflow-y-auto flex flex-col space-y-5">
                  {/* TITOLO CBOT */}
                  <div className="space-y-1">
                    <div className="flex justify-between items-center">
                      <label className="text-xs font-bold text-slate-400 tracking-wider">
                        Nome cBot <span className="text-rose-400 font-bold">*</span>
                      </label>
                      <button type="button" onClick={() => { setWhisperTarget({ fieldKey: 'title', context: 'modal' }); setWhisperOpen(true); }} className="flex items-center space-x-1.5 px-1.5 py-0.5 rounded text-[10px] font-bold border bg-amber-500/10 text-amber-400 border-amber-500/30 hover:bg-amber-500/25">
                        <Mic size={12} /><span>DETTA NOME</span>
                      </button>
                    </div>
                    <input 
                      type="text" value={newTitle} onChange={(e) => setNewTitle(e.target.value)}
                      placeholder="Inserisci un nome testuale (es. SuperTrendRobot)" 
                      className="w-full bg-[#090d16] border border-slate-800 rounded-lg p-2.5 text-xs text-white focus:outline-none focus:border-emerald-500 font-mono"
                    />
                  </div>

                  {/* FILE UPLOAD CTRADER (.cs, .txt) */}
                  <div className="space-y-1">
                    <label className="text-xs font-bold text-slate-400 tracking-wider">Codice Sorgente Algoritmico (.cs)</label>
                    <input type="file" ref={fileInputRefNew} accept=".cs,.txt" onChange={(e) => e.target.files?.[0] && handleFileContentLoad(e.target.files[0], 'new')} className="hidden" />
                    
                    <div 
                      onDragOver={handleDragOver} onDragLeave={handleDragLeave} onDrop={(e) => handleDrop(e, 'new')}
                      onClick={() => fileInputRefNew.current?.click()}
                      className={`border border-dashed rounded-lg p-5 bg-[#090d16] flex flex-col items-center justify-center space-y-1 cursor-pointer transition-all ${
                        isDragging ? 'border-amber-500 bg-amber-500/10' : 'border-slate-800 hover:border-slate-600'
                      }`}
                    >
                      <Upload className={newFileName ? "text-emerald-400" : "text-slate-500"} size={22} />
                      <span className="text-xs text-slate-300 font-mono">
                        {newFileName ? `File selezionato: ${newFileName}` : 'Trascina qui il tuo file C# (.cs) o clicca per esplorare'}
                      </span>
                    </div>
                  </div>

                  {/* FONDAMENTA CTRADER */}
                  <div className="grid grid-cols-2 gap-3 pt-1">
                    {[
                      { id: 'entrata', label: 'ENTRATA' }, { id: 'uscita', label: 'USCITA' },
                      { id: 'stopLoss', label: 'STOP LOSS' }, { id: 'lottaggio', label: 'LOTTAGGIO' },
                      { id: 'parzializzazione', label: 'PARZIALIZZAZIONE/AUMENTO' }, { id: 'trailingStop', label: 'TRAILING STOP' },
                    ].map((item) => (
                      <div key={item.id} className="bg-[#090d16] border border-slate-800 rounded-lg p-3 flex flex-col justify-between h-36">
                        <div className="flex justify-between items-center shrink-0">
                          <span className="text-[11px] font-bold text-emerald-400 tracking-wide font-mono">{item.label}</span>
                          <button type="button" onClick={() => { setWhisperTarget({ item: item.id as any, context: 'modal' } as any); setWhisperOpen(true); }} className="flex items-center space-x-1.5 px-1.5 py-0.5 rounded text-[10px] font-bold border bg-amber-500/10 text-amber-400 border-amber-500/30 hover:bg-amber-500/25">
                            <Mic size={12} /><span>DETTA</span>
                          </button>
                        </div>
                        <textarea 
                          value={newBlocks[item.id as keyof typeof newBlocks]}
                          onChange={(e) => setNewBlocks({...newBlocks, [item.id]: e.target.value})}
                          placeholder="Descrivi la logica finanziaria..."
                          className="w-full bg-transparent text-xs text-slate-200 focus:outline-none resize-none placeholder-slate-700 font-mono mt-1 flex-1"
                        />
                        <div className="flex justify-end pt-1 shrink-0">
                          <button type="button" onClick={() => setExpandedBlock({ label: item.label, value: newBlocks[item.id as keyof typeof newBlocks], isModalTarget: true, fieldKey: item.id })} className="text-slate-600 hover:text-emerald-400 transition-colors flex items-center space-x-1 text-[10px] font-mono">
                            <Maximize2 size={11} /><span>Espandi</span>
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                
              </>
            ) : (
              <>
                <div className="p-6 overflow-y-auto flex flex-col space-y-5">
                  {/* TEXTFIELD TITOLO STRATEGIA PYTHON */}
                  <div className="space-y-1">
                    <div className="flex justify-between items-center">
                      <label className="text-xs font-bold text-slate-400 tracking-wider">
                        Nome Strategia Python <span className="text-rose-400 font-bold">*</span>
                      </label>
                      <button type="button" onClick={() => { setWhisperTarget({ fieldKey: 'title', context: 'modal' }); setWhisperOpen(true); }} className="flex items-center space-x-1.5 px-1.5 py-0.5 rounded text-[10px] font-bold border bg-amber-500/10 text-amber-400 border-amber-500/30 hover:bg-amber-500/25">
                        <Mic size={12} /><span>DETTA NOME</span>
                      </button>
                    </div>
                    <input 
                      type="text" value={newTitle} onChange={(e) => setNewTitle(e.target.value)}
                      placeholder="Inserisci un nome testuale (es. PythonMeanReversion)" 
                      className="w-full bg-[#090d16] border border-slate-800 rounded-lg p-2.5 text-xs text-white focus:outline-none focus:border-emerald-500 font-mono"
                    />
                  </div>

                  {/* FILE UPLOAD PYTHON (.py, .txt) + PULSANTE OMNISTRAPY */}
                  <div className="space-y-2">
                    <div className="flex justify-between items-center">
                      <label className="text-xs font-bold text-slate-400 tracking-wider">Script Python (.py)</label>
                      
                      {/* Pulsante Agente AI per la generazione del codice con Icona Gemini */}
                      <button 
                        type="button" 
                        onClick={() => showToast('warning', 'OmniStraPy: Modulo di generazione codice integrato. Pronto per il collegamento futuro al backend.')}
                        className="flex items-center space-x-1.5 px-2.5 py-1 rounded text-[10px] font-bold border bg-purple-500/10 text-purple-400 border-purple-500/30 hover:bg-purple-500/25 font-mono transition-all shadow-[0_0_15px_rgba(168,85,247,0.05)]"
                      >
                        <GeminiIcon className="h-3 w-3 text-purple-400" />
                        <span>GENERA CON OMNISTRAPY</span>
                      </button>
                    </div>

                    <input type="file" ref={fileInputRefNew} accept=".py,.txt" onChange={(e) => e.target.files?.[0] && handleFileContentLoad(e.target.files[0], 'new')} className="hidden" />
                    
                    <div 
                      onDragOver={handleDragOver} onDragLeave={handleDragLeave} onDrop={(e) => handleDrop(e, 'new')}
                      onClick={() => fileInputRefNew.current?.click()}
                      className={`border border-dashed rounded-lg p-5 bg-[#090d16] flex flex-col items-center justify-center space-y-1 cursor-pointer transition-all ${
                        isDragging ? 'border-amber-500 bg-amber-500/10' : 'border-slate-800 hover:border-slate-600'
                      }`}
                    >
                      <Upload className={newFileName ? "text-amber-400" : "text-slate-500"} size={22} />
                      <span className="text-xs text-slate-300 font-mono">
                        {newFileName ? `File selezionato: ${newFileName}` : 'Trascina qui il tuo file Python (.py) o clicca per esplorare'}
                      </span>
                    </div>
                  </div>

                  {/* 🛠️ NUOVA SEZIONE TITOLO "DESCRIZIONE" + PULSANTE OMNIDESCRSTRAPY ALLINEATO */}
                  <div className="flex justify-between items-center pt-2 border-t border-slate-900">
                    <h3 className="text-xs font-bold text-slate-400 tracking-wider uppercase font-mono">Descrizione</h3>
                    
                    {/* Pulsante OmniDescrStraPy con Icona Gemini, allineato verticalmente e speculare a quello sopra */}
                    <button 
                      type="button" 
                      onClick={() => showToast('warning', "OmniDescrStraPy: Pronto per l'analisi e l'ottimizzazione predittiva delle 6 fondamenta della strategia.")}
                      className="flex items-center space-x-1.5 px-2.5 py-1 rounded text-[10px] font-bold border bg-purple-500/10 text-purple-400 border-purple-500/30 hover:bg-purple-500/25 font-mono transition-all shadow-[0_0_15px_rgba(168,85,247,0.05)]"
                    >
                      <GeminiIcon className="h-3 w-3 text-purple-400" />
                      <span>DESCRIVI CON OMNIDESCRSTRAPY</span>
                    </button>
                  </div>

                  {/* 6 FONDAMENTA PULITE (Senza pulsanti AI singoli nei blocchi) */}
                  <div className="grid grid-cols-2 gap-3 pt-1">
                    {[
                      { id: 'entrata', label: 'ENTRATA' }, { id: 'uscita', label: 'USCITA' },
                      { id: 'stopLoss', label: 'STOP LOSS' }, { id: 'lottaggio', label: 'LOTTAGGIO' },
                      { id: 'parzializzazione', label: 'PARZIALIZZAZIONE/AUMENTO' }, { id: 'trailingStop', label: 'TRAILING STOP' },
                    ].map((item) => (
                      <div key={item.id} className="bg-[#090d16] border border-slate-800 rounded-lg p-3 flex flex-col justify-between h-36">
                        <div className="flex justify-between items-center shrink-0">
                          <span className="text-[11px] font-bold text-amber-500 tracking-wide font-mono">{item.label}</span>
                          
                          {/* Mantiene solo la dettatura vocale standard */}
                          <button type="button" onClick={() => { setWhisperTarget({ item: item.id as any, context: 'modal' } as any); setWhisperOpen(true); }} className="flex items-center space-x-1.5 px-1.5 py-0.5 rounded text-[10px] font-bold border bg-amber-500/10 text-amber-400 border-amber-500/30 hover:bg-amber-500/25 font-mono">
                            <Mic size={11} /><span>DETTA</span>
                          </button>
                        </div>
                        <textarea 
                          value={newBlocks[item.id as keyof typeof newBlocks]}
                          onChange={(e) => setNewBlocks({...newBlocks, [item.id]: e.target.value})}
                          placeholder="Descrivi la logica finanziaria di questo blocco..."
                          className="w-full bg-transparent text-xs text-slate-200 focus:outline-none resize-none placeholder-slate-700 font-mono mt-1 flex-1"
                        />
                        <div className="flex justify-end pt-1 shrink-0">
                          <button type="button" onClick={() => setExpandedBlock({ label: item.label, value: newBlocks[item.id as keyof typeof newBlocks], isModalTarget: true, fieldKey: item.id })} className="text-slate-600 hover:text-amber-500 transition-colors flex items-center space-x-1 text-[10px] font-mono">
                            <Maximize2 size={11} /><span>Espandi</span>
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                
              </>
            )}

            {/* PULSANTI DI CONTROLLO GENERALI SALVATAGGIO */}
            <div className="p-4 border-t border-slate-800/80 bg-[#0d111d] flex justify-end space-x-3 shrink-0">
              <button onClick={() => setIsModalOpen(false)} className="px-4 py-2 bg-slate-800 hover:bg-slate-700 text-slate-300 rounded-lg font-bold text-xs tracking-wider transition-colors font-mono">Annulla</button>
              <button onClick={handleSaveStrategy} className="px-5 py-2 bg-emerald-500 hover:bg-emerald-400 text-slate-950 rounded-lg font-bold text-xs tracking-wider transition-colors font-mono">Aggiungi Strategia</button>
            </div>

          </div>
        </div>
      )}

      {/* OVERLAY FOCUS COMPONENTE ESPANSO */}
      {expandedBlock && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/90 backdrop-blur-md p-6">
          <div className="bg-[#0b0f19] border border-slate-700 rounded-xl w-full max-w-2xl flex flex-col overflow-hidden shadow-2xl font-mono text-left">
            <div className="p-4 bg-slate-900/60 border-b border-slate-800 flex justify-between items-center">
              <span className="text-sm font-bold text-emerald-400 tracking-wider">
                {expandedBlock.label} {(!expandedBlock.isModalTarget && !isEditing) ? '(SOLO LETTURA)' : '(MODIFICA COMPLETA)'}
              </span>
              <button onClick={() => setExpandedBlock(null)} className="text-slate-400 hover:text-white transition-colors p-1 bg-slate-800 rounded-md"><X size={16} /></button>
            </div>
            <div className="p-6 flex-1 flex flex-col space-y-4">
              <textarea
                value={expandedBlock.value}
                readOnly={!expandedBlock.isModalTarget && !isEditing}
                onChange={(e) => {
                  const updatedValue = e.target.value;
                  setExpandedBlock({ ...expandedBlock, value: updatedValue });
                  if (expandedBlock.isModalTarget && expandedBlock.fieldKey) {
                    setNewBlocks(prev => ({ ...prev, [expandedBlock.fieldKey!]: updatedValue }));
                  } else if (expandedBlock.fieldKey) {
                    setStrategieData(prevData => prevData.map(strat => strat.id === selectedStrategyId ? { ...strat, blocks: { ...strat.blocks, [expandedBlock.fieldKey!]: updatedValue } } : strat));
                  }
                }}
                className={`w-full flex-1 border rounded-lg p-4 text-sm text-slate-200 focus:outline-none min-h-[320px] resize-none leading-relaxed font-mono ${
                  (!expandedBlock.isModalTarget && !isEditing) ? 'bg-[#05070f]/50 border-slate-900 cursor-default' : 'bg-[#05070f] border-slate-800 focus:border-amber-500'
                }`}
              />
              <div className="flex justify-between items-center text-xs text-slate-500">
                <span>{(!expandedBlock.isModalTarget && !isEditing) ? "Abilita la modalità modifica sulla scheda madre per scrivere." : "Sincronizzato."}</span>
                {(expandedBlock.isModalTarget || isEditing) && (
                  <button type="button" onClick={() => { if (expandedBlock.fieldKey) { setWhisperTarget({ fieldKey: expandedBlock.fieldKey as any, context: expandedBlock.isModalTarget ? 'modal' : 'expanded' }); setWhisperOpen(true); } }} className="flex items-center space-x-1.5 px-3 py-1.5 rounded text-xs font-bold border bg-amber-500/10 text-amber-400 border-amber-500/30 hover:bg-amber-500/25">
                    <Mic size={14} /><span>PARLA</span>
                  </button>
                )}
              </div>
            </div>
          </div>
        </div>
      )}

      <OmniWhisper isOpen={whisperOpen} onClose={() => { setWhisperOpen(false); setWhisperTarget(null); }} onTextInject={handleWhisperInject} />
    
    {/* MODALE DI ZOOM CODICE A SCHERMO INTERO */}
    {isCodeZoomOpen && currentStrategy && (
    <div className="fixed inset-0 z-50 flex flex-col bg-[#1e1e1e] animate-fade-in font-mono text-xs">
        <div className="bg-[#181818] border-b border-slate-900 px-6 py-4 flex justify-between items-center text-slate-400 select-none shrink-0">
        <div className="flex items-center space-x-4">
            <span className="text-sm font-bold text-white tracking-wider">{currentStrategy.title.toUpperCase()}</span>
            <span className="text-[11px] bg-emerald-500/10 text-emerald-400 border border-emerald-500/20 px-2 py-0.5 rounded font-bold">AMBIENTE DI ANALISI MASSIMIZZATO</span>
        </div>
        <div className="flex items-center space-x-3">
            <button 
                onClick={() => handleCopyToClipboard(currentStrategy.code)}
                className="flex items-center space-x-1.5 px-4 py-2 bg-emerald-500 hover:bg-emerald-400 text-slate-950 rounded-lg font-bold text-xs tracking-wider transition-colors shadow-[0_0_10px_rgba(16,185,129,0.1)]"
            >
                <Copy size={14} /> <span>COPIA CODICE</span>
            </button>
            <button 
                onClick={() => setIsCodeZoomOpen(false)}
                className="flex items-center space-x-1.5 px-4 py-2 bg-slate-800 hover:bg-slate-700 text-white rounded-lg font-bold text-xs tracking-wider transition-colors border border-slate-700"
            >
                <X size={14} /> <span>CHIUDI ZOOM</span>
            </button>
        </div>
        </div>
        
        <div className="flex-1 overflow-auto p-6 bg-[#1e1e1e]">
        <div className="flex space-x-4 min-w-max select-text text-sm">
            <div className="text-[#858585] select-none text-right pr-4 border-r border-[#3c3c3c] space-y-1.5">
            {currentStrategy.code?.split('\n').map((_, i) => <div key={i}>{i + 1}</div>) || <div>1</div>}
            </div>
            <div className="text-[#d4d4d4] space-y-1.5 tracking-wide leading-relaxed">
            {renderHighlightedCode(currentStrategy.code)}
            </div>
        </div>
        </div>
    </div>
    )}
    </div>
  );
}