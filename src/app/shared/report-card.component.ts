import { DatePipe } from '@angular/common';
import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Report } from '../models/report.model';
import { AuthService } from '../services/auth.service';
import { StatusBadgeComponent } from './status-badge.component';

@Component({
  selector: 'app-report-card',
  imports: [RouterLink, DatePipe, StatusBadgeComponent],
  template: `
    <article class="rounded-xl border border-slate-200 bg-white p-4 transition hover:border-slate-300">
      <div class="flex items-start justify-between gap-3">
        <div class="min-w-0">
          <h3 class="truncate font-semibold text-slate-900">{{ report.zoneName }}</h3>
          <p class="truncate text-sm text-slate-600">{{ report.address }}</p>
        </div>
        <app-status-badge [status]="report.status" />
      </div>

      <div class="mt-3 space-y-1 text-xs text-slate-500">
        <p><span class="font-medium text-slate-600">Inició:</span> {{ report.startedAt | date: 'dd/MM/yyyy HH:mm' }}</p>
        <p><span class="font-medium text-slate-600">Vecinos que confirman:</span> {{ report.confirmationCount }}</p>
        @if (report.assignedTechnicianName) {
          <p><span class="font-medium text-slate-600">Técnico:</span> {{ report.assignedTechnicianName }}</p>
        }
      </div>

      <div class="mt-4 flex items-center gap-2">
        <a
          [routerLink]="['/reportes', report.id]"
          class="rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 hover:bg-slate-50"
        >
          Ver detalle
        </a>

        @if (canConfirm()) {
          <button
            type="button"
            (click)="confirmed.emit(report)"
            class="rounded-lg bg-brand-600 px-3 py-1.5 text-sm font-semibold text-white hover:bg-brand-700"
          >
            A mí también me afecta
          </button>
        } @else if (report.confirmedByMe) {
          <span class="text-xs font-medium text-emerald-700">✓ Ya lo confirmaste</span>
        }
      </div>
    </article>
  `,
})
export class ReportCardComponent {
  @Input({ required: true }) report!: Report;
  @Input() allowConfirm = true;
  @Output() confirmed = new EventEmitter<Report>();

  private auth = inject(AuthService);

  // No se puede confirmar un corte cerrado, ni el propio, ni dos veces
  canConfirm(): boolean {
    return (
      this.allowConfirm &&
      this.report.isActive &&
      !this.report.confirmedByMe &&
      this.report.reportedByUserId !== this.auth.userId()
    );
  }
}
