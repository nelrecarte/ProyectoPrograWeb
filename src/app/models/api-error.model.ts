// Todos los errores del backend vienen con esta forma.
// Se revisa el "code" para saber qué pasó, no el mensaje.
export interface ApiError {
  error: string;
  code: string;
  data: any;
}
