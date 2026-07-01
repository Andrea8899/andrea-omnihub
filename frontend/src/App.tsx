// src/App.tsx
import { useState } from 'react';
import { User, Settings, LogOut } from 'lucide-react'; // Aggiunti Settings e LogOut per il dropdown d'élite
import Sidebar from './components/SideBar';
import StrategieView from './views/StrategieView';

export default function App() {
  const [activeTab, setActiveTab] = useState('TRADING');
  const [activeSubTab, setActiveSubTab] = useState('STRATEGIE');
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false);
  
  // STATO PER GESTIRE IL MENU PROFILO INTERATTIVO
  const [isProfileMenuOpen, setIsProfileMenuOpen] = useState(false);

  return (
    <div className="flex h-screen w-screen overflow-hidden bg-[#0a0f1d] text-slate-200 font-mono">
      
      {/* SIDEBAR COMPONENTIZZATA */}
      <Sidebar 
        activeTab={activeTab}
        setActiveTab={setActiveTab}
        isSidebarCollapsed={isSidebarCollapsed}
        setIsSidebarCollapsed={setIsSidebarCollapsed}
      />

      {/* CONTENITORE PRINCIPALE DESTRO */}
      <main className="flex-1 flex flex-col overflow-hidden">
        
        {/* HEADER BAR CON DISMISSIONE PULSANTE STATICO E INTEGRAZIONE DROPDOWN */}
        <header className="h-16 flex items-center justify-end px-8 border-b border-slate-800 bg-[#0a0f1d] z-30">
          <div className="relative">
            <button 
              onClick={() => setIsProfileMenuOpen(!isProfileMenuOpen)}
              className="flex items-center space-x-3 bg-slate-800/30 border border-slate-700 rounded-xl px-4 py-1.5 hover:border-emerald-400 text-left select-none transition-all shadow-inner"
            >
              <div className="h-6 w-6 rounded-full bg-emerald-500/20 border border-emerald-500/40 flex items-center justify-center text-emerald-400 font-bold text-[10px] font-mono">
                AB
              </div>
              <div className="hidden sm:flex flex-col">
                <span className="text-xs font-bold text-white tracking-wide">Andrea B.</span>
                <span className="text-[9px] font-mono text-emerald-400 uppercase tracking-widest font-black">Developer</span>
              </div>
            </button>

            {/* INTERFACCIA MENU UTENTE DROPDOWN */}
            {isProfileMenuOpen && (
              <div className="absolute right-0 mt-2 w-56 bg-[#0f1428] border border-slate-800 rounded-xl shadow-2xl z-50 p-1.5 font-mono text-xs animate-slide-in text-left">
                <div className="px-3 py-2 border-b border-slate-900 mb-1">
                  <p className="text-[10px] text-slate-500">Loggato come:</p>
                  <p className="text-xs font-bold text-slate-200 truncate">andrea@omnihub.local</p>
                </div>
                
                <button 
                  onClick={() => { setIsProfileMenuOpen(false); alert('Apertura Impostazioni Profilo...'); }}
                  className="w-full text-left px-3 py-2 text-slate-300 hover:bg-[#161f3d] hover:text-white rounded-lg flex items-center space-x-2.5 transition-colors"
                >
                  <Settings size={14} className="text-slate-400" />
                  <span>Impostazioni Profilo</span>
                </button>
                
                <div className="border-t border-slate-900 my-1"></div>
                
                <button 
                  onClick={() => { setIsProfileMenuOpen(false); alert('Logout eseguito.'); }}
                  className="w-full text-left px-3 py-2 text-rose-400 hover:bg-rose-500/10 rounded-lg flex items-center space-x-2.5 transition-colors font-bold"
                >
                  <LogOut size={14} />
                  <span>Disconnetti</span>
                </button>
              </div>
            )}
          </div>
        </header>

        {/* Area Operativa Sotto-Menu + View Attiva */}
        <section className="p-8 bg-[#0a0f1d] flex-1 overflow-y-auto flex flex-col space-y-6">
          
          {/* Sub-menu Orizzontale */}
          <div className="w-full bg-[#111830] rounded-lg p-1.5 flex border border-slate-800/80 justify-between items-center shrink-0">
            {['JOURNALING', 'STRATEGIE', 'BACKTESTING', 'LEADERBOARD', 'PROP_FIRM'].map((tab) => (
              <button
                key={tab}
                onClick={() => setActiveSubTab(tab)}
                className={`flex-1 text-center py-2.5 rounded text-xs font-bold tracking-wider transition-all ${
                  activeSubTab === tab
                    ? 'border border-emerald-500/30 text-emerald-400 bg-emerald-500/5'
                    : 'text-slate-400 hover:text-white'
                }`}
              >
                {tab}
              </button>
            ))}
          </div>

          {/* RENDERING CONDIZIONALE DELLE PAGINE */}
          {activeSubTab === 'STRATEGIE' ? (
            <StrategieView />
          ) : (
            <div className="flex-1 border border-slate-800 bg-[#0e1326]/30 rounded-xl flex items-center justify-center text-slate-500 text-xs tracking-widest uppercase">
              PANNELLO: {activeSubTab} IN ATTESA DI AGGIORNAMENTO
            </div>
          )}

        </section>
      </main>
    </div>
  );
}