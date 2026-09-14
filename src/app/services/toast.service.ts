import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  kind: 'success' | 'error';
  text: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  toasts = signal<Toast[]>([]);
  private lastId = 0;

  success(text: string): void {
    this.show('success', text);
  }

  error(text: string): void {
    this.show('error', text);
  }

  remove(id: number): void {
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }

  private show(kind: 'success' | 'error', text: string): void {
    this.lastId = this.lastId + 1;
    const id = this.lastId;
    this.toasts.update((list) => [...list, { id, kind, text }]);
    setTimeout(() => this.remove(id), 5000);
  }
}
