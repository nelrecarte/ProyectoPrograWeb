import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Report } from '../models/report.model';
import { ReportsService } from '../services/reports.service';
import { ToastService } from '../services/toast.service';
import { ReportCardComponent } from '../shared/report-card.component';

@Component({
  selector: 'app-my-reports',
  imports: [RouterLink, ReportCardComponent],
  template: `
    <div class="mb-6 flex flex-wrap items-center justify-between gap-4">
      <h1 class="text-2xl font-bold text-slate-900">Mis reportes</h1>
      <a
        routerLink="/reportes/nuevo"
        class="rounded-lg bg-brand-600 px-4 py-2 text-sm font-semibold text-white hover:bg-brand-700"
      >
        Reportar un corte
      </a>
    </div>

    @if (loading()) {
      <p class="py-10 text-center text-sm text-slate-500">Cargando…</p>
    } @else if (reports().length === 0) {
      <div class="rounded-xl border border-dashed border-slate-300 bg-white px-6 py-12 text-center">
        <p class="text-sm font-medium text-slate-700">Todavía no reportaste ningún corte</p>
      </div>
    } @else {
      <div class="grid gap-4 sm:grid-cols-2">
        @for (report of reports(); track report.id) {
          <!-- Nadie confirma su propio reporte, por eso no se muestra el botón -->
          <app-report-card [report]="report" [allowConfirm]="false" />
        }
      </div>
    }
  `,
})
export class MyReportsComponent implements OnInit {
  private reportsService = inject(ReportsService);
  private toast = inject(ToastService);

  loading = signal(true);
  reports = signal<Report[]>([]);

  ngOnInit(): void {
    this.reportsService.getMine().subscribe({
      next: (reports) => {
        this.reports.set(reports);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.error(err.error?.error ?? 'No se pudieron cargar tus reportes.');
      },
    });
  }
}
