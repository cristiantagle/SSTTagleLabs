import { Construction } from 'lucide-react';
import { useLocation } from 'react-router-dom';

export function PlaceholderPage() {
    const location = useLocation();
    const pageName = location.pathname.substring(1).toUpperCase();

    return (
        <div className="flex flex-col items-center justify-center h-[60vh] text-center space-y-6">
            <div className="w-24 h-24 rounded-full bg-slate-800/50 flex items-center justify-center border border-white/5">
                <Construction size={40} className="text-slate-600" />
            </div>
            <div>
                <h2 className="text-2xl font-bold text-white mb-2">Módulo {pageName}</h2>
                <p className="text-slate-400">Esta sección está en construcción.</p>
            </div>
            <button className="px-6 py-2 rounded-lg bg-cyan-500/10 text-cyan-400 hover:bg-cyan-500/20 border border-cyan-500/20 transition-colors">
                Volver al Dashboard
            </button>
        </div>
    );
}
