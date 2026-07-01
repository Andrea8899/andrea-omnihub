// src/components/Sidebar.tsx
import { useState, useEffect } from 'react';
import { TrendingUp, Cpu, CheckSquare, Calendar, ChevronLeft, ChevronRight, Database } from 'lucide-react';
import OmniHubLogo from './OmniHubLogo';

interface SidebarProps {
  activeTab: string;
  setActiveTab: (tab: string) => void;
  isSidebarCollapsed: boolean;
  setIsSidebarCollapsed: (collapsed: boolean) => void;
}

export default function Sidebar({ 
  activeTab, 
  setActiveTab, 
  isSidebarCollapsed, 
  setIsSidebarCollapsed 
}: SidebarProps) {
  const [isLogoHovered, setIsLogoHovered] = useState(false);
  
  // --- STATI DI SINCRO REAL-TIME ---
  const [dbConnected, setDbConnected] = useState<boolean>(false);
  const [gitConnected, setGitConnected] = useState<boolean>(true); // Sincronizzazione GitHub attiva di base

  // Monitoraggio dello stato del backend Python
  useEffect(() => {
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

    checkHealth();
    const interval = setInterval(checkHealth, 10000);
    return () => clearInterval(interval);
  }, []);

  const menuItems = [
    { id: 'TRADING', label: 'TRADING', icon: <TrendingUp size={20} /> },
    { id: 'AI_AGENT', label: 'AI AGENT', icon: <Cpu size={20} /> },
    { id: 'ROUTINE', label: 'ROUTINE', icon: <CheckSquare size={20} /> },
    { id: 'PLANNING', label: 'PLANNING', icon: <Calendar size={20} /> },
  ];

  return (
    <aside className={`bg-[#0e1326] flex flex-col justify-between p-4 border-r border-slate-800 transition-all duration-300 relative ${
      isSidebarCollapsed ? 'w-20' : 'w-64'
    }`}>
      <div>
        {/* Header Sidebar */}
        <div className="flex items-center justify-between mb-10 pt-2 px-2 h-10">
          {!isSidebarCollapsed ? (
            <>
              <div className="flex items-center space-x-3 overflow-hidden">
                <OmniHubLogo />
                <span className="text-xl font-bold tracking-widest text-white whitespace-nowrap">
                  OMNIHUB
                </span>
              </div>
              <button 
                onClick={() => setIsSidebarCollapsed(true)}
                className="p-1.5 rounded-md bg-slate-800/50 hover:bg-slate-700 text-slate-400 hover:text-white transition-colors border border-slate-700/30"
              >
                <ChevronLeft size={18} />
              </button>
            </>
          ) : (
            <div 
              className="mx-auto w-10 h-10 flex items-center justify-center cursor-pointer relative"
              onMouseEnter={() => setIsLogoHovered(true)}
              onMouseLeave={() => setIsLogoHovered(false)}
            >
              {isLogoHovered ? (
                <button 
                  onClick={() => {
                    setIsSidebarCollapsed(false);
                    setIsLogoHovered(false);
                  }}
                  className="p-1.5 rounded-md bg-slate-800 text-slate-200 hover:bg-slate-700 hover:text-white transition-all duration-150 border border-slate-600 animate-fade-in"
                >
                  <ChevronRight size={18} />
                </button>
              ) : (
                <div className="animate-fade-in">
                  <OmniHubLogo />
                </div>
              )}
            </div>
          )}
        </div>

        {/* Voci di Navigazione */}
        <nav className="space-y-4">
          {menuItems.map((item) => {
            const isActive = activeTab === item.id;
            return (
              <div key={item.id} className="relative group">
                <button 
                  onClick={() => setActiveTab(item.id)}
                  className={`flex items-center space-x-3 w-full p-2.5 rounded-lg transition-all text-left ${
                    isActive 
                      ? 'text-emerald-400 font-semibold bg-emerald-500/5 border border-emerald-500/10' 
                      : 'text-slate-400 hover:text-white hover:bg-slate-800/30'
                  } ${isSidebarCollapsed ? 'justify-center' : ''}`}
                >
                  <div className="shrink-0">{item.icon}</div>
                  {!isSidebarCollapsed && <span className="text-sm tracking-wider">{item.label}</span>}
                </button>

                {/* Tooltip in modalità compressa */}
                {isSidebarCollapsed && (
                  <div className="absolute left-full top-1/2 -translate-y-1/2 ml-3 px-3 py-1.5 bg-[#111830] text-emerald-400 text-xs font-bold tracking-widest border border-emerald-500/20 rounded shadow-xl opacity-0 scale-95 pointer-events-none group-hover:opacity-100 group-hover:scale-100 transition-all duration-150 z-50 whitespace-nowrap">
                    {item.label}
                  </div>
                )}
              </div>
            );
          })}
        </nav>
      </div>
      
      {/* Footer Sidebar: STATUS PANEL (DB + GITHUB) */}
      <div className="flex flex-col space-y-3 pt-4 border-t border-slate-800/60 px-2">
        
        {/* STATO DATABASE */}
        <div className="relative group flex items-center">
          <div className={`flex items-center w-full transition-colors ${isSidebarCollapsed ? 'justify-center' : 'space-x-3'} ${dbConnected ? 'text-emerald-400' : 'text-slate-600'}`}>
            <Database size={18} className="shrink-0" />
            {!isSidebarCollapsed && (
              <div className="flex items-center justify-between flex-1 text-[10px] font-mono tracking-wider font-bold">
                <span>LOCAL DB</span>
                <span className={`px-1.5 py-0.5 rounded text-[9px] ${dbConnected ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20' : 'bg-slate-800 text-slate-500 border border-slate-700'}`}>
                  {dbConnected ? 'CONNESSO' : 'DISCONNESSO'}
                </span>
              </div>
            )}
          </div>
          {isSidebarCollapsed && (
            <div className={`absolute left-full top-1/2 -translate-y-1/2 ml-3 px-3 py-1.5 bg-[#111830] text-xs font-bold font-mono border rounded shadow-xl opacity-0 scale-95 pointer-events-none group-hover:opacity-100 group-hover:scale-100 transition-all duration-150 z-50 whitespace-nowrap ${dbConnected ? 'text-emerald-400 border-emerald-500/20' : 'text-slate-500 border-slate-800'}`}>
              DATABASE: {dbConnected ? 'CONNESSO' : 'DISCONNESSO'}
            </div>
          )}
        </div>

        {/* STATO GITHUB */}
        <div className="relative group flex items-center">
          <div className={`flex items-center w-full transition-colors ${isSidebarCollapsed ? 'justify-center' : 'space-x-3'} ${gitConnected ? 'text-emerald-400' : 'text-slate-600'}`}>
            <svg viewBox="0 0 24 24" className="h-[18px] w-[18px] fill-current shrink-0" aria-hidden="true">
              <path d="M12 2C6.477 2 2 6.477 2 12c0 4.42 2.865 8.166 6.839 9.489.5.092.682-.217.682-.482 0-.237-.008-.866-.013-1.7-2.782.603-3.369-1.34-3.369-1.34-.454-1.156-1.11-1.462-1.11-1.462-.908-.62.069-.608.069-.608 1.003.07 1.531 1.03 1.531 1.03.892 1.529 2.341 1.087 2.91.831.092-.646.35-1.086.636-1.336-2.22-.253-4.555-1.11-4.555-4.943 0-1.091.39-1.984 1.029-2.683-.103-.253-.446-1.27.098-2.647 0 0 .84-.269 2.75 1.024A9.564 9.564 0 0112 6.844c.85.004 1.705.115 2.504.337 1.909-1.293 2.747-1.024 2.747-1.024.546 1.377.203 2.394.1 2.647.64.699 1.028 1.592 1.028 2.683 0 3.842-2.339 4.687-4.566 4.935.359.309.678.919.678 1.852 0 1.336-.012 2.415-.012 2.743 0 .267.18.579.688.481C19.137 20.162 22 16.418 22 12c0-5.523-4.477-10-10-10z" />
            </svg>
            {!isSidebarCollapsed && (
              <div className="flex items-center justify-between w-full text-[10px] font-mono tracking-wider font-bold">
                <span>GITHUB SYNC</span>
                <span className={`px-1.5 py-0.5 rounded text-[9px] ${gitConnected ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20' : 'bg-slate-800 text-slate-500 border border-slate-700'}`}>
                  {gitConnected ? 'CONNESSO' : 'DISCONNESSO'}
                </span>
              </div>
            )}
          </div>
          {isSidebarCollapsed && (
            <div className={`absolute left-full top-1/2 -translate-y-1/2 ml-3 px-3 py-1.5 bg-[#111830] text-xs font-bold font-mono border rounded shadow-xl opacity-0 scale-95 pointer-events-none group-hover:opacity-100 group-hover:scale-100 transition-all duration-150 z-50 whitespace-nowrap ${gitConnected ? 'text-emerald-400 border-emerald-500/20' : 'text-slate-500 border-slate-800'}`}>
              GITHUB: {gitConnected ? 'CONNESSO' : 'DISCONNESSO'}
            </div>
          )}
        </div>

      </div>
    </aside>
  );
}