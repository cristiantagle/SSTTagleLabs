import { useState, useEffect } from 'react';
import { Dashboard } from './pages/Dashboard';
import { EmpresasPage } from './pages/Empresas';
import { PlaceholderPage } from './pages/Placeholder';
import { Search, Bell, MonitorCheck, LayoutDashboard, Building2, Users, FileText, ShieldAlert, ClipboardCheck, Activity, Settings } from 'lucide-react';

type Page = 'dashboard' | 'empresas' | 'trabajadores' | 'documentos' | 'matriz-riesgos' | 'siniestros' | 'auditoria' | 'configuracion';

const menuItems: { icon: typeof LayoutDashboard; label: string; id: Page }[] = [
  { icon: LayoutDashboard, label: 'Dashboard', id: 'dashboard' },
  { icon: Building2, label: 'Empresas', id: 'empresas' },
  { icon: Users, label: 'Trabajadores', id: 'trabajadores' },
  { icon: FileText, label: 'Documentos', id: 'documentos' },
  { icon: ShieldAlert, label: 'Matriz Riesgos', id: 'matriz-riesgos' },
  { icon: Activity, label: 'Siniestros', id: 'siniestros' },
  { icon: ClipboardCheck, label: 'Auditoría', id: 'auditoria' },
  { icon: Settings, label: 'Configuración', id: 'configuracion' },
];

function App() {
  const [currentPage, setCurrentPage] = useState<Page>('dashboard');
  const [apiStatus, setApiStatus] = useState(false);

  useEffect(() => {
    const check = () => fetch('http://localhost:5000/api/status').then(() => setApiStatus(true)).catch(() => setApiStatus(false));
    check();
    const interval = setInterval(check, 10000);
    return () => clearInterval(interval);
  }, []);

  const getTitle = (page: Page) => {
    const item = menuItems.find(m => m.id === page);
    return item?.label || 'Dashboard';
  };

  const renderPage = () => {
    switch (currentPage) {
      case 'dashboard': return <Dashboard />;
      case 'empresas': return <EmpresasPage />;
      default: return <PlaceholderPage />;
    }
  };

  return (
    <div className="flex min-h-screen bg-slate-950 text-slate-200">
      {/* Sidebar */}
      <aside className="w-64 bg-slate-900 border-r border-white/5 flex flex-col h-screen fixed left-0 top-0 z-30">
        <div className="h-16 flex items-center gap-3 px-6 border-b border-white/5">
          <div className="w-8 h-8 rounded-lg bg-gradient-to-tr from-cyan-500 to-blue-600 flex items-center justify-center">
            <span className="font-bold text-white text-sm">TL</span>
          </div>
          <span className="font-bold text-lg text-white">SST Tagle</span>
        </div>

        <nav className="flex-1 py-6 px-3 space-y-1 overflow-y-auto">
          <div className="px-3 mb-2 text-xs font-semibold text-slate-500 uppercase">Principal</div>
          {menuItems.map((item) => (
            <button
              key={item.id}
              onClick={() => setCurrentPage(item.id)}
              className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all ${currentPage === item.id
                  ? 'bg-cyan-500/10 text-cyan-400 border border-cyan-500/20'
                  : 'text-slate-400 hover:text-white hover:bg-white/5 border border-transparent'
                }`}
            >
              <item.icon size={20} />
              <span className="font-medium text-sm">{item.label}</span>
            </button>
          ))}
        </nav>

        <div className="p-4 border-t border-white/5">
          <div className="flex items-center gap-3 p-3 rounded-xl bg-slate-800/50">
            <div className="w-8 h-8 rounded-full bg-slate-700 flex items-center justify-center">
              <span className="text-xs font-bold text-slate-300">CT</span>
            </div>
            <div>
              <p className="text-sm font-medium text-white">Cristian Tagle</p>
              <p className="text-xs text-slate-500">Admin</p>
            </div>
          </div>
        </div>
      </aside>

      {/* Main Content */}
      <div className="flex-1 ml-64 flex flex-col">
        <header className="h-16 sticky top-0 z-20 bg-slate-950/80 backdrop-blur-md border-b border-white/5 px-8 flex items-center justify-between">
          <h2 className="text-xl font-bold text-white">{getTitle(currentPage)}</h2>
          <div className="flex items-center gap-6">
            <div className="relative">
              <Search size={18} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
              <input type="text" placeholder="Buscar..." className="bg-slate-900 border border-white/5 rounded-full pl-10 pr-4 py-1.5 text-sm focus:outline-none focus:border-cyan-500/50 w-64" />
            </div>
            <div className="flex items-center gap-3">
              <div className={`p-2 rounded-full border ${apiStatus ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-400' : 'bg-red-500/10 border-red-500/20 text-red-400'}`}>
                <MonitorCheck size={18} />
              </div>
              <button className="p-2 rounded-full hover:bg-white/5 text-slate-400 relative">
                <Bell size={18} />
                <span className="absolute top-1.5 right-2 w-2 h-2 bg-pink-500 rounded-full"></span>
              </button>
            </div>
          </div>
        </header>

        <main className="flex-1 p-8 overflow-y-auto">
          {renderPage()}
        </main>
      </div>
    </div>
  );
}

export default App;
