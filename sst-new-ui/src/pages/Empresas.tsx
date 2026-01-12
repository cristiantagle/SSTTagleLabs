import { useEffect, useState } from 'react';
import { Empresa } from '../types';
import { Plus, Search, Building2, MapPin, Users, MoreVertical } from 'lucide-react';

export function EmpresasPage() {
    const [empresas, setEmpresas] = useState<Empresa[]>([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');

    useEffect(() => {
        fetch('http://localhost:5000/api/empresas')
            .then(res => res.json())
            .then(data => {
                setEmpresas(data);
                setLoading(false);
            })
            .catch(err => {
                console.error("Error cargando empresas:", err);
                setLoading(false);
            });
    }, []);

    const filtered = empresas.filter(e =>
        e.razonSocial.toLowerCase().includes(searchTerm.toLowerCase()) ||
        e.rut.includes(searchTerm)
    );

    return (
        <div className="space-y-6 animate-fade-in">
            {/* Header Actions */}
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <div>
                    <h1 className="text-3xl font-bold text-white tracking-tight">Empresas</h1>
                    <p className="text-slate-400 mt-1">Gestión de clientes y centros de trabajo</p>
                </div>

                <button className="flex items-center gap-2 px-5 py-2.5 bg-cyan-600 hover:bg-cyan-500 text-white rounded-xl font-medium transition-colors shadow-lg shadow-cyan-900/20">
                    <Plus size={20} />
                    Nueva Empresa
                </button>
            </div>

            {/* Filters */}
            <div className="relative">
                <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-500" size={20} />
                <input
                    type="text"
                    placeholder="Buscar por Razón Social o RUT..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="w-full bg-slate-900 border border-white/10 rounded-xl py-3 pl-12 pr-4 text-slate-200 focus:outline-none focus:border-cyan-500/50 focus:ring-1 focus:ring-cyan-500/50 transition-all"
                />
            </div>

            {/* Grid Content */}
            {loading ? (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {[1, 2, 3].map(i => (
                        <div key={i} className="h-48 rounded-2xl bg-slate-900/50 border border-white/5 animate-pulse"></div>
                    ))}
                </div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {filtered.map(empresa => (
                        <div key={empresa.id} className="group relative bg-slate-900/40 border border-white/5 rounded-2xl p-6 hover:bg-slate-900/80 hover:border-cyan-500/30 transition-all duration-300 hover:-translate-y-1">

                            <div className="flex justify-between items-start mb-4">
                                <div className="w-12 h-12 rounded-xl bg-blue-500/10 flex items-center justify-center text-blue-400">
                                    {empresa.logoPath ? (
                                        <img src={empresa.logoPath} alt="Logo" className="w-full h-full object-cover rounded-xl" />
                                    ) : (
                                        <Building2 size={24} />
                                    )}
                                </div>
                                <button className="p-2 text-slate-500 hover:text-white rounded-lg hover:bg-white/5 transition-colors">
                                    <MoreVertical size={20} />
                                </button>
                            </div>

                            <h3 className="text-xl font-bold text-white mb-1">{empresa.razonSocial}</h3>
                            <p className="text-sm font-mono text-cyan-400 mb-4">{empresa.rut}</p>

                            <div className="space-y-2 text-sm text-slate-400">
                                <div className="flex items-center gap-2">
                                    <MapPin size={16} className="text-slate-600" />
                                    <span className="truncate">{empresa.direccion || 'Sin dirección'}</span>
                                </div>
                                <div className="flex items-center gap-2">
                                    <Users size={16} className="text-slate-600" />
                                    <span>{empresa.numeroTrabajadores} Trabajadores</span>
                                </div>
                            </div>

                            <div className="mt-6 pt-4 border-t border-white/5 flex gap-2">
                                <button className="flex-1 py-2 rounded-lg bg-white/5 hover:bg-white/10 text-sm font-medium text-slate-300 transition-colors">
                                    Ver Detalles
                                </button>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}
