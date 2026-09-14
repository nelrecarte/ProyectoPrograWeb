import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Report } from '../models/report.model';
import { Zone } from '../models/zone.model';
import { AuthService } from '../services/auth.service';
import { ReportsService } from '../services/reports.service';
import { ToastService } from '../services/toast.service';
import { ZonesService } from '../services/zones.service';
import { ReportCardComponent } from '../shared/report-card.component';

@Component({
  selector: 'app-home',
  imports: [RouterLink, ReportCardComponent],
  template: `
    <section class="mb-8 flex flex-wrap items-end justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-slate-900">Hola, {{ auth.displayName() }}</h1>
        <p class="mt-1 text-sm text-slate-600">
          @if (myZone(); as zone) {
            Tu zona es <span class="font-medium text-slate-900">{{ zone.name }}</span
            >.
            @if (zone.activeReportId) {
              Ahora mismo tiene un corte abierto.
            } @else {
              No hay cortes abiertos ahí.
            }
          } @else {
            Todavía no elegiste tu zona.
          }
        </p>
      </div>

      <a
        routerLink="/reportes/nuevo"
        class="rounded-lg bg-brand-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-brand-700"
      >
        Reportar un corte
      </a>
    </section>

    @if (loading()) {
      <p class="py-10 text-center text-sm text-slate-500">Cargando…</p>
    } @else {
      <h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-500">
        Cortes abiertos ({{ reports().length }})
      </h2>

      @if (reports().length === 0) {
        <div class="rounded-xl border border-dashed border-slate-300 bg-white px-6 py-12 text-center">
          <p class="text-sm font-medium text-slate-700">No hay cortes abiertos</p>
          <p class="mt-1 text-sm text-slate-500">Cuando alguien reporte uno, va a aparecer acá.</p>
        </div>
      } @else {
        <div class="grid gap-4 sm:grid-cols-2">
          @for (report of reports(); track report.id) {
            <app-report-card [report]="report" (confirmed)="confirm($event)" />
          }
        </div>
      }
    }
  `,
})
export class HomeComponent implements OnInit {
  auth = inject(AuthService);
  private zonesService = inject(ZonesService);
  private reportsService = inject(ReportsService);
  private toast = inject(ToastService);

  loading = signal(true);
  zones = signal<Zone[]>([]);
  reports = signal<Report[]>([]);

  ngOnInit(): void {
    this.zonesService.getAll(true).subscribe({
      next: (zones) => this.zones.set(zones),
      error: () => this.zones.set([]),
    });

    this.reportsService.getAll({ isActive: true }).subscribe({
      next: (reports) => {
        this.reports.set(reports);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.error(err.error?.error ?? 'No se pudieron cargar los reportes.');
      },
    });
  }

  myZone(): Zone | null {
    const zoneId = this.auth.user()?.zoneId;
    if (!zoneId) {
      return null;
    }
    return this.zones().find((zone) => zone.id === zoneId) ?? null;
  }

  confirm(report: Report): void {
    this.reportsService.confirm(report.id).subscribe({
      next: (updated) => {
        this.reports.update((list) => list.map((r) => (r.id === updated.id ? updated : r)));
        this.toast.success('Gracias, tu confirmación ayuda a priorizar el corte.');
      },
      error: (err) => this.toast.error(err.error?.error ?? 'No se pudo confirmar.'),
    });
  }
}
