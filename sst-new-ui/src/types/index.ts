export interface Empresa {
    id: number;
    rut: string;
    razonSocial: string;
    giro: string;
    rubroPrincipal?: string;
    numeroTrabajadores: number;
    mutual?: string;
    direccion?: string;
    telefono?: string;
    emailContacto?: string;
    logoPath?: string;
}

export interface Trabajador {
    id: number;
    rut: string;
    nombres: string;
    apellidos: string;
    cargo: string;
    email?: string;
    empresaId: number;
}
