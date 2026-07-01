// src/views/StrategieView.tsx
import { useState, useEffect, useRef } from 'react';
import { 
  Plus, Search, Code, FileText, Database, Edit3, Trash2, 
  ArrowUpDown, Maximize2, X, Mic, Upload, Check, Lock, Unlock, RotateCcw, AlertTriangle, FileCode, Copy
} from 'lucide-react';
import OmniWhisper from '../components/OmniWhisper';

const GithubIcon = ({ className = "h-8 w-8" }: { className?: string }) => (
  <svg viewBox="0 0 24 24" className={`fill-current ${className}`}>
    <path d="M12 2C6.477 2 2 6.477 2 12c0 4.42 2.865 8.166 6.839 9.489.5.092.682-.217.682-.482 0-.237-.008-.866-.013-1.7-2.782.603-3.369-1.34-3.369-1.34-.454-1.156-1.11-1.462-1.11-1.462-.908-.62.069-.608.069-.608 1.003.07 1.531 1.03 1.531 1.03.892 1.529 2.341 1.087 2.91.831.092-.646.35-1.086.636-1.336-2.22-.253-4.555-1.11-4.555-4.943 0-1.091.39-1.984 1.029-2.683-.103-.253-.446-1.27.098-2.647 0 0 .84-.269 2.75 1.024A9.564 9.564 0 0112 6.844c.85.004 1.705.115 2.504.337 1.909-1.293 2.747-1.024 2.747-1.024.546 1.377.203 2.394.1 2.647.64.699 1.028 1.592 1.028 2.683 0 3.842-2.339 4.687-4.566 4.935.359.309.678.919.678 1.852 0 1.336-.012 2.415-.012 2.743 0 .267.18.579.688.481C19.137 20.162 22 16.418 22 12c0-5.523-4.477-10-10-10z" />
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
        setStrategieData(data);
        if (data.length > 0 && !selectedStrategyId) {
          setSelectedStrategyId(data[0].id);
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
        showToast('success', 'Strategia aggiornata nel database con successo.');
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
        showToast('success', 'Nuova strategia creata e indicizzata con successo!');
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
        showToast('success', 'Strategia rimossa definitivamente dal database.');
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
    .filter(s => s.title.toLowerCase().includes(searchQuery.toLowerCase()))
    .sort((a, b) => {
      if (sortOption === 'alpha-asc') return a.title.localeCompare(b.title);
      if (sortOption === 'alpha-desc') return b.title.localeCompare(a.title);
      if (sortOption === 'date-desc') return new Date(b.created || b.id).getTime() - new Date(a.created || a.id).getTime();
      if (sortOption === 'date-asc') return new Date(a.created || a.id).getTime() - new Date(b.created || b.id).getTime();
      return 0;
    });

  const currentStrategy = strategieData.find(s => s.id === selectedStrategyId) || strategieData[0];

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
            <Database size={13} /> <span>DATABASE LOCALE</span>
          </span>
          <span className={`font-bold px-2 py-0.5 rounded text-[10px] ${dbConnected ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20' : 'bg-slate-800 text-slate-500 border border-slate-700'}`}>
            {dbConnected ? 'CONNESSO' : 'DISCONNESSO'}
          </span>
        </div>
        <div className="flex items-center justify-between">
          <span className="text-slate-500 flex items-center space-x-1.5">
            <GithubIcon className="h-3.5 w-3.5" /> <span>GITHUB SYNC</span>
          </span>
          <span className={`font-bold px-2 py-0.5 rounded text-[10px] ${gitConnected ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20' : 'bg-slate-800 text-slate-500 border border-slate-700'}`}>
            {gitConnected ? 'CONNESSO' : 'DISCONNESSO'}
          </span>
        </div>
      </div>

      {/* HEADER BAR */}
      <div className="flex justify-between items-center shrink-0 bg-[#0c1020] p-3 rounded-xl border border-slate-900 shadow-lg">
        <button 
          onClick={() => setIsModalOpen(true)}
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

          <div className="space-y-3">
            {filteredAndSortedStrategies.map((strat) => {
              const isSelected = strat.id === selectedStrategyId;
              return (
                <div 
                  key={strat.id}
                  onClick={() => { setSelectedStrategyId(strat.id); setIsEditing(false); }}
                  className={`p-4 rounded-xl border transition-all cursor-pointer flex flex-col space-y-4 ${
                    isSelected ? 'border-emerald-500 bg-[#162244] shadow-[0_0_15px_rgba(16,185,129,0.1)]' : 'border-slate-800 bg-[#0f152d] hover:border-slate-700 hover:bg-[#141d3d]'
                  }`}
                >
                  <div className="flex items-center justify-between">
                    <span className="text-md font-bold text-white tracking-wide">{strat.title}</span>
                    <div className="flex items-center space-x-3">
                      <Database className={`h-8 w-8 transition-colors ${strat.isSavedDB && dbConnected ? 'text-emerald-400' : 'text-slate-600'}`} />
                      <GithubIcon className={strat.isSavedGit && gitConnected ? 'text-emerald-400' : 'text-slate-600'} />
                    </div>
                  </div>
                  <div className="space-y-1.5 text-xs text-slate-400 font-medium tracking-wide">
                    <div>Creata il: <span className="text-white font-bold font-mono bg-slate-900/60 px-1.5 py-0.5 rounded">{strat.createdDisplay || '...'}</span></div>
                    <div>Modifica il: <span className="text-white font-bold font-mono bg-slate-900/60 px-1.5 py-0.5 rounded">{strat.modified || '...'}</span></div>
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
                <div className="flex items-center space-x-3">
                  
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
                              <button onClick={() => { setWhisperTarget({ fieldKey: block.key as any, context: 'view' }); setWhisperOpen(true); }} className="flex items-center space-x-1 px-2 py-0.5 rounded text-[10px] font-bold transition-all border bg-amber-500/10 text-amber-400 border-amber-500/30 hover:bg-amber-500/35">
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

      {/* MODALE DI CREAZIONE NUOVA STRATEGIA */}
      {isModalOpen && (
        <div className="fixed inset-0 z-40 flex items-center justify-center bg-slate-950/80 backdrop-blur-sm p-4">
          <div className="bg-[#111625] border border-slate-800 rounded-xl w-full max-w-3xl max-h-[95vh] flex flex-col overflow-hidden shadow-2xl">
            <div className="p-5 border-b border-slate-800/80 flex justify-between items-center shrink-0">
              <h2 className="text-lg font-bold text-white tracking-wider font-mono">Aggiungi Nuova Strategia</h2>
              <button onClick={() => setIsModalOpen(false)} className="text-slate-400 hover:text-white transition-colors"><X size={20} /></button>
            </div>

            <div className="p-6 overflow-y-auto flex flex-col space-y-5">
              <div className="space-y-1">
                <div className="flex justify-between items-center">
                  <label className="text-xs font-bold text-slate-400 tracking-wider">
                    Nome Strategia <span className="text-rose-400 font-bold">*</span>
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

              <div className="space-y-1">
                <label className="text-xs font-bold text-slate-400 tracking-wider">Codice Sorgente Algoritmico</label>
                <input type="file" ref={fileInputRefNew} accept=".cs,.py,.txt" onChange={(e) => e.target.files?.[0] && handleFileContentLoad(e.target.files[0], 'new')} className="hidden" />
                
                <div 
                  onDragOver={handleDragOver} onDragLeave={handleDragLeave} onDrop={(e) => handleDrop(e, 'new')}
                  onClick={() => fileInputRefNew.current?.click()}
                  className={`border border-dashed rounded-lg p-5 bg-[#090d16] flex flex-col items-center justify-center space-y-1 cursor-pointer transition-all ${
                    isDragging ? 'border-amber-500 bg-amber-500/10' : 'border-slate-800 hover:border-slate-600'
                  }`}
                >
                  <Upload className={newFileName ? "text-emerald-400" : "text-slate-500"} size={22} />
                  <span className="text-xs text-slate-300 font-mono">
                    {newFileName ? `File selezionato: ${newFileName}` : 'Trascina qui il tuo file o clicca per esplorare'}
                  </span>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3 pt-1">
                {[
                  { id: 'entrata', label: 'ENTRATA' }, { id: 'uscita', label: 'USCITA' },
                  { id: 'stopLoss', label: 'STOP LOSS' }, { id: 'lottaggio', label: 'LOTTAGGIO' },
                  { id: 'parzializzazione', label: 'PARZIALIZZAZIONE/AUMENTO' }, { id: 'trailingStop', label: 'TRAILING STOP' },
                ].map((item) => (
                  <div key={item.id} className="bg-[#090d16] border border-slate-800 rounded-lg p-3 flex flex-col justify-between h-36">
                    <div className="flex justify-between items-center shrink-0">
                      <span className="text-[11px] font-bold text-emerald-400 tracking-wide font-mono">{item.label}</span>
                      <button type="button" onClick={() => { setWhisperTarget({ fieldKey: item.id as any, context: 'modal' }); setWhisperOpen(true); }} className="flex items-center space-x-1.5 px-1.5 py-0.5 rounded text-[10px] font-bold border bg-amber-500/10 text-amber-400 border-amber-500/30 hover:bg-amber-500/25">
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