// El backend guarda las fechas en UTC y toma como UTC cualquier fecha que
// llegue sin zona horaria. Por eso lo que sale de un input se convierte antes
// de mandarlo.

// '1999-05-04' (input type="date") -> '1999-05-04T00:00:00.000Z'
export function dateToIso(value: string): string {
  return new Date(value + 'T00:00:00Z').toISOString();
}

// '2026-09-13T10:30' (input type="datetime-local") -> ISO en UTC
export function localToIso(value: string): string | undefined {
  if (!value) {
    return undefined;
  }
  return new Date(value).toISOString();
}
