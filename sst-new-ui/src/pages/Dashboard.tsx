import { useEffect, useState } from 'react';
import { Building2, Users, FileText, ShieldAlert, ArrowUpRight } from 'lucide-react';

interface DashboardStats {
    empresas: number;
    trabajadores: number;
    documentos: number;
    matrices: number;
}

export function Dashboard() {
    const [stats, setStats] = useState<DashboardStats | null>(null);
    const [dbPath, setDbPath] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        // Cargar Status y Stats en paralelo
        Promise.all([
            fetch('http://localhost:5000/api/status').then(r => r.json()).catch(() => null),
            fetch('http://localhost:5000/api/dashboard/summary').then(r => r.json()).catch(() => null)
        ]).then(([statusData, statsData]) => {
            if (statusData) setDbPath(statusData.database);
            if (statsData) setStats(statsData);
            setLoading(false);
        });
    }, []);

    const statCards = [
        { label: 'Empresas', value: stats?.empresas, icon: Building2, color: 'text-blue-400', bg: 'bg-blue-500/10', border: 'border-blue-500/20' },
        { label: 'Trabajadores', value: stats?.trabajadores, icon: Users, color: 'text-violet-400', bg: 'bg-violet-500/10', border: 'border-violet-500/20' },
        { label: 'Documentos', value: stats?.documentos, icon: FileText, color: 'text-amber-400', bg: 'bg-amber-500/10', border: 'border-amber-500/20' },
        { label: 'Matrices Riesgo', value: stats?.matrices, icon: ShieldAlert, color: 'text-pink-400', bg: 'bg-pink-500/10', border: 'border-pink-500/20' },
    ];

    return (
        <div className="space-y-8 animate-fade-in">
            {/* Welcome Hero */}
            <div className="relative rounded-3xl bg-gradient-to-br from-indigo-950 via-slate-900 to-cyan-950/30 border border-white/5 p-10 overflow-hidden shadow-2xl">
                <div className="absolute top-0 right-0 w-96 h-96 bg-cyan-500/10 rounded-full blur-3xl -translate-y-1/2 translate-x-1/2 pointer-events-none"></div>

                <div className="relative z-10 flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
                    <div>
                        <h1 className="text-4xl font-bold text-white mb-2 tracking-tight">
                            Hola, <span className="text-cyan-400">Cristian</span> 👋
                        </h1>
                        <p className="text-slate-400 max-w-xl text-lg">
                            Bienvenido al panel <b>SST Tagle Labs</b>.
                            <br />Resumen de tu gestión de seguridad en tiempo real.
                        </p>
                    </div>

                    <div className="flex flex-col items-end gap-2">
                        <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-emerald-500/10 text-emerald-400 border border-emerald-500/20 text-xs font-bold uppercase tracking-wider">
                            <span className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse"></span>
                            Sistema Online
                        </div>
                        {dbPath && (
                            <span className="text-xs text-slate-500 font-mono" title={dbPath}>
                                BD Local Conectada
                            </span>
                        )}
                    </div>
                </div>
            </div>

            {/* Stats Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                {statCards.map((item, idx) => (
                    <div key={idx} className={`relative group p-6 rounded-2xl border ${item.border} ${item.bg} hover:bg-opacity-20 transition-all duration-300 hover:-translate-y-1`}>
                        <div className="flex justify-between items-start mb-4">
                            <div className={`p-3 rounded-xl bg-slate-950/50 ${item.color}`}>
                                <item.icon size={24} />
                            </div>
                            <ArrowUpRight size={16} className="text-slate-500 group-hover:text-white transition-colors" />
                        </div>

                        <p className="text-slate-400 text-sm font-medium mb-1">{item.label}</p>

                        {loading ? (
                            <div className="h-8 w-16 bg-slate-700/50 rounded animate-pulse"></div>
                        ) : (
                            <h3 className="text-3xl font-bold text-white tracking-tight">
                                {item.value !== undefined ? item.value : 0}
                            </h3>
                        )}
                    </div>
                ))}
            </div>

            {/* Activity Section Placeholder */}
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                <div className="lg:col-span-2 rounded-2xl bg-slate-900/50 border border-white/5 p-6 min-h-[300px]">
                    <h3 className="text-lg font-bold text-white mb-4">Actividad Reciente</h3>
                    <div className="flex items-center justify-center h-full text-slate-500 text-sm">
                        Próximamente: Gráfico de documentos generados
                    </div>
                </div>

                <div className="rounded-2xl bg-slate-900/50 border border-white/5 p-6">
                    <h3 className="text-lg font-bold text-white mb-4">Accesos Rápidos</h3>
                    <div className="space-y-3">
                        {['Nueva Empresa', 'Registrar Trabajador', 'Emitir ODI'].map(action => (
                            <button key={action} className="w-full text-left px-4 py-3 rounded-xl bg-slate-800/50 hover:bg-slate-800 border border-white/5 hover:border-cyan-500/30 transition-all text-sm text-slate-300 hover:text-cyan-400 flex items-center justify-between group">
                                {action}
                                <ArrowUpRight size={14} className="opacity-0 group-hover:opacity-100 transition-opacity" />
                            </button>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
}
