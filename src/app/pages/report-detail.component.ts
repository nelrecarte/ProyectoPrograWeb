import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Report } from '../models/report.model';
import { Technician } from '../models/technician.model';
import { AuthService } from '../services/auth.service';
import { ReportsService } from '../services/reports.service';
import { TechniciansService } from '../services/technicians.service';
import { ToastService } from '../services/toast.service';
import { localToIso } from '../shared/date-utils';
import { StatusBadgeComponent } from '../shared/status-badge.component';

@Component({
  selector: 'app-report-detail',
  imports: [ReactiveFormsModule, RouterLink, DatePipe, DecimalPipe, StatusBadgeComponent],
  template: `
    @if (loading()) {
      <p class="py-10 text-center text-sm text-slate-500">Cargando…</p>
    } @else if (report(); as r) {
      <a routerLink="/reportes" class="text-sm text-slate-500 hover:underline">← Volver a reportes</a>

      <header class="mt-3 flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 class="text-2xl font-bold text-slate-900">{{ r.zoneName }}</h1>
          <p class="text-sm text-slate-600">{{ r.address }}</p>
        </div>
        <app-status-badge [status]="r.status" />
      </header>

      <div class="mt-6 grid gap-6 lg:grid-cols-3">
        <section class="space-y-4 lg:col-span-2">
          <div class="rounded-xl border border-slate-200 bg-white p-5">
            <h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-500">Datos del corte</h2>
            <dl class="grid gap-3 text-sm sm:grid-cols-2">
              <div>
                <dt class="text-slate-500">Inició</dt>
                <dd class="font-medium text-slate-900">{{ r.startedAt | date: 'dd/MM/yyyy HH:mm' }}</dd>
              </div>
              <div>
                <dt class="text-slate-500">Reportado por</dt>
                <dd class="font-medium text-slate-900">{{ r.reportedByName || '—' }}</dd>
              </div>
              <div>
                <dt class="text-slate-500">Confirmaciones</dt>
                <dd class="font-medium text-slate-900">{{ r.confirmationCount }}</dd>
              </div>
              <div>
                <dt class="text-slate-500">Técnico asignado</dt>
                <dd class="font-medium text-slate-900">{{ r.assignedTechnicianName || 'Sin asignar' }}</dd>
              </div>
              @if (r.resolvedAt) {
                <div>
                  <dt class="text-slate-500">Restablecido</dt>
                  <dd class="font-medium text-slate-900">{{ r.resolvedAt | date: 'dd/MM/yyyy HH:mm' }}</dd>
                </div>
              }
              @if (r.evidenceUrl) {
                <div class="sm:col-span-2">
                  <dt class="text-slate-500">Evidencia</dt>
                  <dd>
                    <a [href]="r.evidenceUrl" target="_blank" rel="noopener" class="text-brand-700 underline">
                      Abrir enlace
                    </a>
                  </dd>
                </div>
              }
            </dl>
          </div>

          @if (r.resolution; as res) {
            <div class="rounded-xl border border-emerald-200 bg-emerald-50 p-5">
              <h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-emerald-700">Resolución</h2>
              <dl class="grid gap-3 text-sm sm:grid-cols-2">
                <div>
                  <dt class="text-emerald-700">Causa</dt>
                  <dd class="font-medium text-emerald-950">{{ res.cause }}</dd>
                </div>
                <div>
                  <dt class="text-emerald-700">Resuelto por</dt>
                  <dd class="font-medium text-emerald-950">{{ res.technicianName }}</dd>
                </div>
                <div>
                  <dt class="text-emerald-700">Duró</dt>
                  <dd class="font-medium text-emerald-950">
                    {{ res.resolutionMinutes | number: '1.0-0' }} min
                  </dd>
                </div>
                <div>
                  <dt class="text-emerald-700">Restablecido</dt>
                  <dd class="font-medium text-emerald-950">{{ res.restoredAt | date: 'dd/MM/yyyy HH:mm' }}</dd>
                </div>
                <div class="sm:col-span-2">
                  <dt class="text-emerald-700">Detalle</dt>
                  <dd class="text-emerald-950">{{ res.detail }}</dd>
                </div>
              </dl>
            </div>
          }

          <!-- El técnico cierra el corte -->
          @if (canResolve()) {
            <form
              [formGroup]="resolutionForm"
              (ngSubmit)="resolve()"
              class="space-y-4 rounded-xl border border-slate-200 bg-white p-5"
            >
              <h2 class="text-sm font-semibold uppercase tracking-wide text-slate-500">Registrar resolución</h2>
              <p class="text-xs text-slate-500">Una vez registrada no se puede modificar.</p>

              <label class="block text-sm">
                <span class="mb-1 block font-medium text-slate-700">Causa</span>
                <input formControlName="cause" class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm" />
              </label>

              <label class="block text-sm">
                <span class="mb-1 block font-medium text-slate-700">Detalle</span>
                <textarea
                  rows="3"
                  formControlName="detail"
                  class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
                ></textarea>
                @if (resolutionForm.controls.detail.touched && resolutionForm.controls.detail.invalid) {
                  <span class="mt-1 block text-xs text-red-600">Al menos 10 caracteres.</span>
                }
              </label>

              <div class="grid gap-4 sm:grid-cols-2">
                <label class="block text-sm">
                  <span class="mb-1 block font-medium text-slate-700">Minutos estimados</span>
                  <input
                    type="number"
                    formControlName="estimatedMinutes"
                    class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
                  />
                </label>
                <label class="block text-sm">
                  <span class="mb-1 block font-medium text-slate-700">Hora de restablecimiento</span>
                  <input
                    type="datetime-local"
                    formControlName="restoredAt"
                    class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
                  />
                </label>
              </div>

              <button
                type="submit"
                [disabled]="saving()"
                class="rounded-lg bg-emerald-600 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-700 disabled:opacity-60"
              >
                Cerrar el corte
              </button>
            </form>
          }
        </section>

        <aside>
          <div class="rounded-xl border border-slate-200 bg-white p-5">
            <h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-500">Acciones</h2>
            <div class="space-y-3">
              @if (canConfirm()) {
                <button
                  type="button"
                  [disabled]="saving()"
                  (click)="confirm()"
                  class="w-full rounded-lg bg-brand-600 px-4 py-2 text-sm font-semibold text-white hover:bg-brand-700 disabled:opacity-60"
                >
                  A mí también me afecta
                </button>
              } @else if (r.confirmedByMe) {
                <p class="text-sm text-emerald-700">✓ Ya confirmaste este corte.</p>
              }

              @if (canAccept()) {
                <button
                  type="button"
                  [disabled]="saving()"
                  (click)="accept()"
                  class="w-full rounded-lg bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800 disabled:opacity-60"
                >
                  Tomar este reporte
                </button>
              }

              @if (canChangeStatus()) {
                <div class="flex gap-2">
                  <button
                    type="button"
                    [disabled]="saving()"
                    (click)="changeStatus('en_verificacion')"
                    class="flex-1 rounded-lg border border-slate-300 px-3 py-2 text-xs font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-60"
                  >
                    En verificación
                  </button>
                  <button
                    type="button"
                    [disabled]="saving()"
                    (click)="changeStatus('confirmado')"
                    class="flex-1 rounded-lg border border-slate-300 px-3 py-2 text-xs font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-60"
                  >
                    Confirmado
                  </button>
                </div>
              }

              @if (auth.isAdmin() && r.isActive) {
                <div class="border-t border-slate-100 pt-3">
                  <label class="block text-sm">
                    <span class="mb-1 block font-medium text-slate-700">Asignar técnico</span>
                    <select #tecnico class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm">
                      <option value="">Elegí un técnico…</option>
                      @for (tech of technicians(); track tech.id) {
                        <option [value]="tech.id">
                          {{ tech.fullName }} — {{ tech.zoneName }} ({{ tech.activeReportCount }} activos)
                        </option>
                      }
                    </select>
                  </label>
                  <button
                    type="button"
                    [disabled]="saving()"
                    (click)="assign(tecnico.value)"
                    class="mt-2 w-full rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-60"
                  >
                    Asignar
                  </button>
                </div>
              }

              @if (!r.isActive) {
                <p class="text-sm text-slate-500">Este corte ya está cerrado.</p>
              }
            </div>
          </div>
        </aside>
      </div>
    } @else {
      <div class="rounded-xl border border-slate-200 bg-white p-8 text-center">
        <p class="font-medium text-slate-900">No se pudo cargar el reporte.</p>
        <a routerLink="/reportes" class="mt-3 inline-block text-sm text-brand-700 underline">Volver a la lista</a>
      </div>
    }
  `,
})
export class ReportDetailComponent implements OnInit {
  auth = inject(AuthService);
  private route = inject(ActivatedRoute);
  private reportsService = inject(ReportsService);
  private techniciansService = inject(TechniciansService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);

  reportId = this.route.snapshot.paramMap.get('id') ?? '';

  loading = signal(true);
  saving = signal(false);
  report = signal<Report | null>(null);
  technicians = signal<Technician[]>([]);

  resolutionForm = this.fb.nonNullable.group({
    cause: ['', [Validators.required, Validators.minLength(3)]],
    detail: ['', [Validators.required, Validators.minLength(10)]],
    estimatedMinutes: [60, Validators.required],
    restoredAt: [''],
  });

  ngOnInit(): void {
    this.load();

    if (this.auth.isAdmin()) {
      this.techniciansService.getAll().subscribe({
        next: (techs) => this.technicians.set(techs),
        error: () => this.technicians.set([]),
      });
    }
  }

  canConfirm(): boolean {
    const r = this.report();
    return !!r && r.isActive && !r.confirmedByMe && r.reportedByUserId !== this.auth.userId();
  }

  canAccept(): boolean {
    const r = this.report();
    return !!r && this.auth.isTecnico() && r.isActive && !r.assignedTechnicianId;
  }

  canChangeStatus(): boolean {
    const r = this.report();
    return !!r && r.isActive && (this.auth.isAdmin() || this.auth.isTecnico());
  }

  canResolve(): boolean {
    const r = this.report();
    return !!r && this.auth.isTecnico() && r.isActive && r.resolution === null;
  }

  confirm(): void {
    this.saving.set(true);
    this.reportsService.confirm(this.reportId).subscribe({
      next: (updated) => this.done(updated, 'Confirmado. Gracias por avisar.'),
      error: (err) => this.failed(err, 'No se pudo confirmar.'),
    });
  }

  accept(): void {
    this.saving.set(true);
    this.reportsService.accept(this.reportId).subscribe({
      next: (updated) => this.done(updated, 'Tomaste el reporte.'),
      error: (err) => this.failed(err, 'No se pudo tomar el reporte.'),
    });
  }

  changeStatus(status: string): void {
    this.saving.set(true);
    this.reportsService.changeStatus(this.reportId, status).subscribe({
      next: (updated) => this.done(updated, 'Estado actualizado.'),
      error: (err) => this.failed(err, 'No se pudo cambiar el estado.'),
    });
  }

  assign(technicianId: string): void {
    if (!technicianId) {
      this.toast.error('Elegí un técnico primero.');
      return;
    }

    this.saving.set(true);
    this.reportsService.assign(this.reportId, technicianId).subscribe({
      next: (updated) => this.done(updated, 'Técnico asignado.'),
      error: (err) => this.failed(err, 'No se pudo asignar el técnico.'),
    });
  }

  resolve(): void {
    if (this.resolutionForm.invalid) {
      this.resolutionForm.markAllAsTouched();
      return;
    }

    const values = this.resolutionForm.getRawValue();
    this.saving.set(true);

    this.reportsService
      .resolve(this.reportId, {
        cause: values.cause,
        detail: values.detail,
        estimatedMinutes: Number(values.estimatedMinutes),
        restoredAt: localToIso(values.restoredAt),
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.toast.success('Corte cerrado. Gracias.');
          this.resolutionForm.reset({ cause: '', detail: '', estimatedMinutes: 60, restoredAt: '' });
          this.load();
        },
        error: (err) => this.failed(err, 'No se pudo registrar la resolución.'),
      });
  }

  private load(): void {
    this.loading.set(true);
    this.reportsService.getById(this.reportId).subscribe({
      next: (report) => {
        this.report.set(report);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.report.set(null);
        this.toast.error(err.error?.error ?? 'No se pudo cargar el reporte.');
      },
    });
  }

  private done(updated: Report, message: string): void {
    this.saving.set(false);
    this.report.set(updated);
    this.toast.success(message);
  }

  private failed(err: any, fallback: string): void {
    this.saving.set(false);
    this.toast.error(err.error?.error ?? fallback);
  }
}
