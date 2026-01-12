import { useState } from 'react'

function App() {
  const [count, setCount] = useState(0)

  return (
    <div className="flex flex-col items-center justify-center min-h-screen w-full bg-slate-950 text-white p-8">
      {/* Header Premium */}
      <div className="absolute top-0 w-full p-6 flex justify-between items-center border-b border-white/10 bg-slate-900/50 backdrop-blur-md">
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
        <div className="px-4 py-1.5 rounded-full bg-emerald-500/10 border border-emerald-500/20 text-emerald-400 text-xs font-bold uppercase tracking-wider">
          Sistema Híbrido Activo
        </div>
      </div>

      {/* Main Content */}
      <main className="text-center space-y-8 max-w-2xl relative">
        <div className="absolute -top-20 -left-20 w-64 h-64 bg-cyan-500/10 rounded-full blur-3xl rounded-full mix-blend-screen animate-blob"></div>
        <div className="absolute -bottom-20 -right-20 w-64 h-64 bg-violet-500/10 rounded-full blur-3xl rounded-full mix-blend-screen animate-blob animation-delay-2000"></div>

        <h2 className="text-5xl font-bold tracking-tight">
          Bienvenido a la <span className="text-cyan-400">Nueva Era</span>
        </h2>
        <p className="text-lg text-slate-400 leading-relaxed">
          Esta es tu nueva interfaz base construida con
          <span className="text-white font-semibold mx-1">React</span>,
          <span className="text-white font-semibold mx-1">Vite</span> y
          <span className="text-white font-semibold mx-1">TailwindCSS</span>.
          Lista para integrarse con tu backend .NET vía Tauri.
        </p>

        <div className="grid grid-cols-2 gap-4 mt-8">
          <button
            onClick={() => setCount((c) => c + 1)}
            className="group relative px-8 py-4 bg-slate-900 rounded-2xl border border-white/10 hover:border-cyan-500/50 transition-all hover:shadow-[0_0_40px_-10px_rgba(6,182,212,0.3)] overflow-hidden"
          >
            <div className="absolute inset-0 bg-gradient-to-r from-cyan-500/10 to-blue-500/10 opacity-0 group-hover:opacity-100 transition-opacity"></div>
            <span className="relative font-mono text-xl font-bold text-cyan-400 group-hover:text-white transition-colors">
              Click me: {count}
            </span>
          </button>

          <div className="p-6 rounded-2xl bg-white/5 border border-white/5 flex flex-col items-center justify-center">
            <span className="text-3xl mb-2">🚀</span>
            <span className="text-sm font-semibold text-slate-300">Rendimiento Nativo</span>
          </div>
        </div>
      </main>

      <footer className="absolute bottom-6 text-xs text-slate-600 font-mono">
        powered by Tauri Hybrid Architecture
      </footer>
    </div>
  )
}

export default App
