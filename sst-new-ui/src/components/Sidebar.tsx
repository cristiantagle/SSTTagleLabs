import {
    LayoutDashboard,
    Building2,
    Users,
    FileText,
    ShieldAlert,
    ClipboardCheck,
    Activity,
    Settings,
    X
} from 'lucide-react';
import { NavLink } from 'react-router-dom';

const menuItems = [
    { icon: LayoutDashboard, label: 'Dashboard', path: '/' },
    { icon: Building2, label: 'Empresas', path: '/empresas' },
    { icon: Users, label: 'Trabajadores', path: '/trabajadores' },
    { icon: FileText, label: 'Documentos', path: '/documentos' },
    { icon: ShieldAlert, label: 'Matriz Riesgos', path: '/matriz-riesgos' },
    { icon: Activity, label: 'Siniestros', path: '/siniestros' },
    { icon: ClipboardCheck, label: 'Auditoría', path: '/auditoria' },
    { icon: Settings, label: 'Configuración', path: '/configuracion' },
];

export function Sidebar() {
    return (
        <aside className="w-64 bg-slate-900 border-r border-white/5 flex flex-col h-screen fixed left-0 top-0 z-30">
            {/* Logo Area */}
            <div className="h-16 flex items-center gap-3 px-6 border-b border-white/5 bg-slate-900/50 backdrop-blur-md">
                <div className="w-8 h-8 rounded-lg bg-gradient-to-tr from-cyan-500 to-blue-600 flex items-center justify-center shadow-lg shadow-cyan-500/20">
                    <span className="font-bold text-white text-sm">TL</span>
                </div>
                <span className="font-bold text-lg bg-gradient-to-r from-white to-slate-400 bg-clip-text text-transparent">
                    SST Tagle
                </span>
            </div>

            {/* Navigation */}
            <nav className="flex-1 overflow-y-auto py-6 px-3 space-y-1">
                <div className="px-3 mb-2 text-xs font-semibold text-slate-500 uppercase tracking-wider">
                    Principal
                </div>

                {menuItems.map((item) => (
                    <NavLink
                        key={item.path}
                        to={item.path}
                        className={({ isActive }) => `
              flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all duration-200 group
              ${isActive
                                ? 'bg-cyan-500/10 text-cyan-400 border border-cyan-500/20 shadow-[0_0_20px_-5px_rgba(6,182,212,0.3)]'
                                : 'text-slate-400 hover:text-white hover:bg-white/5 border border-transparent'}
            `}
                    >
                        <item.icon size={20} className="transition-transform group-hover:scale-110" />
                        <span className="font-medium text-sm">{item.label}</span>

                        {/* Active Indicator Strip */}
                        <NavLink to={item.path} className={({ isActive }) =>
                            isActive ? "absolute right-0 w-1 h-8 bg-cyan-500 rounded-l-full opacity-100" : "hidden"
                        }>
                        </NavLink>
                    </NavLink>
                ))}
            </nav>

            {/* User / Footer */}
            <div className="p-4 border-t border-white/5">
                <div className="flex items-center gap-3 p-3 rounded-xl bg-slate-800/50 border border-white/5">
                    <div className="w-8 h-8 rounded-full bg-slate-700 flex items-center justify-center">
                        <span className="text-xs font-bold text-slate-300">CT</span>
                    </div>
                    <div className="flex-1 overflow-hidden">
                        <p className="text-sm font-medium text-white truncate">Cristian Tagle</p>
                        <p className="text-xs text-slate-500 truncate">Admin</p>
                    </div>
                </div>
            </div>
        </aside>
    );
}
