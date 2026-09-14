import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Report, ReportFilters } from '../models/report.model';
import { Zone } from '../models/zone.model';
import { ReportsService } from '../services/reports.service';
import { ToastService } from '../services/toast.service';
import { ZonesService } from '../services/zones.service';
import { ReportCardComponent } from '../shared/report-card.component';

@Component({
  selector: 'app-report-list',
  imports: [ReactiveFormsModule, RouterLink, ReportCardComponent],
  template: `
    <div class="mb-6 flex flex-wrap items-center justify-between gap-4">
      <h1 class="text-2xl font-bold text-slate-900">Reportes</h1>
      <a
        routerLink="/reportes/nuevo"
        class="rounded-lg bg-brand-600 px-4 py-2 text-sm font-semibold text-white hover:bg-brand-700"
      >
        Reportar un corte
      </a>
    </div>

    <form
      [formGroup]="form"
      (ngSubmit)="load()"
      class="mb-6 grid gap-3 rounded-xl border border-slate-200 bg-white p-4 sm:grid-cols-4"
    >
      <label class="text-sm">
        <span class="mb-1 block font-medium text-slate-700">Zona</span>
        <select formControlName="zoneId" class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm">
          <option value="">Todas</option>
          @for (zone of zones(); track zone.id) {
            <option [value]="zone.id">{{ zone.name }}</option>
          }
        </select>
      </label>

      <label class="text-sm">
        <span class="mb-1 block font-medium text-slate-700">Estado</span>
        <select formControlName="status" class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm">
          <option value="">Todos</option>
          @for (status of statuses; track status.value) {
            <option [value]="status.value">{{ status.label }}</option>
          }
        </select>
      </label>

      <label class="text-sm">
        <span class="mb-1 block font-medium text-slate-700">Actividad</span>
        <select formControlName="activity" class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm">
          <option value="abiertos">Solo abiertos</option>
          <option value="resueltos">Solo resueltos</option>
          <option value="">Todos</option>
        </select>
      </label>

      <div class="flex items-end">
        <button
          type="submit"
          class="w-full rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50"
        >
          Filtrar
        </button>
      </div>
    </form>

    @if (loading()) {
      <p class="py-10 text-center text-sm text-slate-500">Cargando…</p>
    } @else if (reports().length === 0) {
      <div class="rounded-xl border border-dashed border-slate-300 bg-white px-6 py-12 text-center">
        <p class="text-sm font-medium text-slate-700">No hay reportes con esos filtros</p>
      </div>
    } @else {
      <div class="grid gap-4 sm:grid-cols-2">
        @for (report of reports(); track report.id) {
          <app-report-card [report]="report" (confirmed)="confirm($event)" />
        }
      </div>
    }
  `,
})
export class ReportListComponent implements OnInit {
  private fb = inject(FormBuilder);
  private reportsService = inject(ReportsService);
  private zonesService = inject(ZonesService);
  private toast = inject(ToastService);

  loading = signal(true);
  zones = signal<Zone[]>([]);
  reports = signal<Report[]>([]);

  statuses = [
    { value: 'nuevo', label: 'Nuevo' },
    { value: 'en_verificacion', label: 'En verificación' },
    { value: 'confirmado', label: 'Confirmado' },
    { value: 'resuelto', label: 'Resuelto' },
  ];

  form = this.fb.nonNullable.group({
    zoneId: '',
    status: '',
    activity: 'abiertos',
  });

  ngOnInit(): void {
    this.zonesService.getAll().subscribe({
      next: (zones) => this.zones.set(zones),
      error: () => this.zones.set([]),
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);

    const values = this.form.getRawValue();
    const filters: ReportFilters = {
      zoneId: values.zoneId || undefined,
      status: values.status || undefined,
      isActive: values.activity === '' ? undefined : values.activity === 'abiertos',
    };

    this.reportsService.getAll(filters).subscribe({
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

  confirm(report: Report): void {
    this.reportsService.confirm(report.id).subscribe({
      next: (updated) => {
        this.reports.update((list) => list.map((r) => (r.id === updated.id ? updated : r)));
        this.toast.success('Confirmado. Gracias por avisar.');
      },
      error: (err) => this.toast.error(err.error?.error ?? 'No se pudo confirmar.'),
    });
  }
}
