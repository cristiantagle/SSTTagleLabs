# SST Tagle Labs - Nueva Interfaz Híbrida

Este proyecto representa la nueva generación de la interfaz de usuario para **SST Tagle Labs**, construida sobre una arquitectura híbrida moderna.

## 🚀 Tecnologías

*   **Frontend:** React 19 + TypeScript
*   **Build Tool:** Vite
*   **Estilos:** TailwindCSS v4
*   **Desktop Wrapper:** Tauri v2 (Pendiente de configuración final)
*   **Backend:** .NET 8 (Existente en `../TagleLabsGestorSST.Services`)

## 🛠️ Cómo iniciar el entorno de desarrollo

1.  Asegúrate de tener **Node.js** instalado.
2.  Navega a esta carpeta:
    ```bash
    cd sst-new-ui
    ```
3.  Instala dependencias (si aún no lo has hecho):
    ```bash
    npm install
    ```
4.  Inicia el servidor de desarrollo:
    ```bash
    npm run dev
    ```
5.  Abre [http://localhost:5173](http://localhost:5173) en tu navegador.

## 📦 Estado Actual

Actualmente, este proyecto es un **esqueleto de Frontend**.
Para convertirlo en una aplicación de escritorio nativa completa, se requiere:

1.  Instalar **Rust** en el sistema.
2.  Ejecutar `npm run tauri init`.
3.  Configurar la comunicación con los servicios .NET existentes.

## 🎨 Diseño

El diseño sigue un lineamiento "Premium Dark", utilizando:
*   Fondos oscuros profundos (`slate-950`).
*   Acatos en Cyan y Emerald.
*   Efectos de vidrio (Glassmorphism) y gradientes sutiles.
