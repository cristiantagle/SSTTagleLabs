import { Outlet, useLocation } from 'react-router-dom';
import { Sidebar } from '../components/Sidebar';
import { Search, Bell, MonitorCheck } from 'lucide-react';
import { useState, useEffect } from 'react';

export function MainLayout() {
    const location = useLocation();
    const [apiStatus, setApiStatus] = useState<boolean>(false);

    // Check API status periodically
    useEffect(() => {
        const checkApi = () => {
            fetch('http://localhost:5000/api/status')
                .then(() => setApiStatus(true))
                .catch(() => setApiStatus(false));
        };

        checkApi();
        const interval = setInterval(checkApi, 10000);
        return () => clearInterval(interval);
    }, []);

    // Format title from path
    const getPageTitle = (path: string) => {
        if (path === '/') return 'Dashboard';
        const segment = path.substring(1);
        return segment.charAt(0).toUpperCase() + segment.slice(1).replace('-', ' ');
    };

    return (
        <div className="flex min-h-screen bg-slate-950 text-slate-200 font-sans selection:bg-cyan-500/30">
            {/* Sidebar - Fixed */}
            <Sidebar />

            {/* Main Content Wrapper */}
            <div className="flex-1 ml-64 flex flex-col min-w-0">

                {/* Top Header */}
                <header className="h-16 sticky top-0 z-20 bg-slate-950/80 backdrop-blur-md border-b border-white/5 px-8 flex items-center justify-between">
                    <h2 className="text-xl font-bold text-white tracking-tight">
                        {getPageTitle(location.pathname)}
                    </h2>

                    <div className="flex items-center gap-6">
                        {/* Search Bar */}
                        <div className="relative group">
                            <Search size={18} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500 group-focus-within:text-cyan-400 transition-colors" />
                            <input
                                type="text"
                                placeholder="Buscar..."
                                className="bg-slate-900 border border-white/5 rounded-full pl-10 pr-4 py-1.5 text-sm focus:outline-none focus:border-cyan-500/50 focus:ring-1 focus:ring-cyan-500/50 w-64 transition-all"
                            />
                        </div>

                        {/* Actions */}
                        <div className="flex items-center gap-3">
                            {/* API Status Indicator */}
                            <div
                                title={apiStatus ? "Backend Connected" : "Backend Offline"}
                                className={`p-2 rounded-full border transition-all ${apiStatus ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-400' : 'bg-red-500/10 border-red-500/20 text-red-400'}`}
                            >
                                <MonitorCheck size={18} />
                            </div>

                            <button className="p-2 rounded-full hover:bg-white/5 text-slate-400 hover:text-white transition-colors relative">
                                <Bell size={18} />
                                <span className="absolute top-1.5 right-2 w-2 h-2 bg-pink-500 rounded-full border-2 border-slate-950"></span>
                            </button>
                        </div>
                    </div>
                </header>

                {/* Page Content */}
                <main className="flex-1 p-8 overflow-y-auto">
                    <Outlet />
                </main>

            </div>
        </div>
    );
}
