import { Component, inject } from '@angular/core';
import { ToastService } from '../services/toast.service';

@Component({
  selector: 'app-toast-host',
  template: `
    <div class="pointer-events-none fixed inset-x-0 bottom-4 z-50 flex flex-col items-center gap-2 px-4">
      @for (toast of toastService.toasts(); track toast.id) {
        <div
          class="pointer-events-auto flex w-full max-w-md items-start gap-3 rounded-lg px-4 py-3 text-sm shadow-lg ring-1"
          [class]="
            toast.kind === 'error'
              ? 'bg-red-50 text-red-800 ring-red-200'
              : 'bg-emerald-50 text-emerald-800 ring-emerald-200'
          "
        >
          <span class="flex-1">{{ toast.text }}</span>
          <button type="button" class="opacity-60 hover:opacity-100" (click)="toastService.remove(toast.id)">
            ✕
          </button>
        </div>
      }
    </div>
  `,
})
export class ToastHostComponent {
  toastService = inject(ToastService);
}
