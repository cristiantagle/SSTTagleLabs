import { useState, useEffect } from 'react'

function App() {
  const [count, setCount] = useState(0)
  const [apiStatus, setApiStatus] = useState<{ status: string; system: string; database: string } | null>(null)

  useEffect(() => {
    // Conexión con Backend .NET
    fetch('http://localhost:5000/api/status')
      .then(res => res.json())
      .then(data => {
        console.log("Datos recibidos de API:", data)
        setApiStatus(data)
      })
      .catch(err => {
        console.error("Error conectando a API:", err)
      })
  }, [])

  return (
    <div className="flex flex-col items-center justify-center min-h-screen w-full bg-slate-950 text-white p-8 overflow-hidden font-sans">
      {/* Header Premium */}
      <div className="absolute top-0 w-full p-6 flex justify-between items-center border-b border-white/10 bg-slate-900/50 backdrop-blur-md z-20">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-cyan-500 to-blue-600 flex items-center justify-center shadow-lg shadow-cyan-500/20">
            <span className="font-bold text-xl">TL</span>
          </div>
          <div>
            <h1 className="text-xl font-bold bg-gradient-to-r from-white to-slate-400 bg-clip-text text-transparent">
              SST Tagle Labs
            </h1>
            <p className="text-xs text-slate-400 font-mono tracking-wider">NEXT GEN UI</p>
          </div>
        </div>

        <div className="flex gap-3">
          {/* Indicador de Backend */}
          <div className={`px-4 py-1.5 rounded-full border text-xs font-bold uppercase tracking-wider flex items-center gap-2 transition-colors duration-500 ${apiStatus ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-400' : 'bg-red-500/10 border-red-500/20 text-red-400'}`}>
            <div className={`w-2 h-2 rounded-full ${apiStatus ? 'bg-emerald-400 animate-pulse' : 'bg-red-400'}`}></div>
            {apiStatus ? '.NET CONNECTED' : 'API OFFLINE'}
          </div>

          <div className="px-4 py-1.5 rounded-full bg-cyan-500/10 border border-cyan-500/20 text-cyan-400 text-xs font-bold uppercase tracking-wider">
            Tauri v2
          </div>
        </div>
      </div>

      {/* Main Content */}
      <main className="text-center space-y-8 max-w-2xl relative z-10">
        <div className="absolute -top-20 -left-20 w-64 h-64 bg-cyan-500/10 rounded-full blur-3xl mix-blend-screen animate-blob"></div>
        <div className="absolute -bottom-20 -right-20 w-64 h-64 bg-violet-500/10 rounded-full blur-3xl mix-blend-screen animate-blob animation-delay-2000"></div>

        <h2 className="text-5xl font-bold tracking-tight">
          Bienvenido a la <span className="text-cyan-400">Nueva Era</span>
        </h2>

        {apiStatus ? (
          <div className="inline-block p-6 rounded-2xl bg-slate-900/80 border border-emerald-500/30 text-left shadow-2xl backdrop-blur-sm animate-fade-in-up">
            <div className="flex items-center gap-2 mb-2 text-emerald-400 font-bold border-b border-white/10 pb-2">
              <span>⚡ CONEXIÓN EXITOSA</span>
            </div>
            <div className="space-y-1 text-sm font-mono text-slate-300">
              <p>📡 <span className="text-slate-500">Sistema:</span> {apiStatus.system}</p>
              <p>💾 <span className="text-slate-500">Base de Datos:</span> <span className="text-amber-400">SQLite Local</span></p>
              <p className="text-xs text-slate-500 truncate max-w-md mt-2 opacity-50">{apiStatus.database}</p>
            </div>
          </div>
        ) : (
          <div className="inline-block p-4 rounded-xl bg-red-900/20 border border-red-500/30 text-red-400 text-sm animate-pulse">
            ⏳ Esperando conexión con API...
          </div>
        )}

        <p className="text-lg text-slate-400 leading-relaxed max-w-lg mx-auto">
          Esta interfaz está conectada en tiempo real con tu lógica de negocio
          <span className="text-white font-semibold mx-1">.NET 8</span> y
          <span className="text-white font-semibold mx-1">SQLite</span>.
        </p>

        <div className="grid grid-cols-2 gap-4 mt-8">
          <button
            onClick={() => setCount((c) => c + 1)}
            className="group relative px-8 py-4 bg-slate-900 rounded-2xl border border-white/10 hover:border-cyan-500/50 transition-all hover:shadow-[0_0_40px_-10px_rgba(6,182,212,0.3)] overflow-hidden cursor-pointer"
          >
            <div className="absolute inset-0 bg-gradient-to-r from-cyan-500/10 to-blue-500/10 opacity-0 group-hover:opacity-100 transition-opacity"></div>
            <span className="relative font-mono text-xl font-bold text-cyan-400 group-hover:text-white transition-colors">
              Click me: {count}
            </span>
          </button>

          <div className="p-6 rounded-2xl bg-white/5 border border-white/5 flex flex-col items-center justify-center hover:bg-white/10 transition-colors cursor-pointer group">
            <span className="text-3xl mb-2 group-hover:scale-110 transition-transform">🚀</span>
            <span className="text-sm font-semibold text-slate-300">Rendimiento Nativo</span>
          </div>
        </div>
      </main>

      <footer className="absolute bottom-6 text-xs text-slate-600 font-mono z-20">
        powered by Tauri Hybrid Architecture
      </footer>
    </div>
  )
}

export default App
