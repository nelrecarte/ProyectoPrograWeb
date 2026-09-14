import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { DuplicateReportData } from '../models/report.model';
import { Zone } from '../models/zone.model';
import { ReportsService } from '../services/reports.service';
import { ToastService } from '../services/toast.service';
import { ZonesService } from '../services/zones.service';
import { localToIso } from '../shared/date-utils';

@Component({
  selector: 'app-report-create',
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="mx-auto max-w-2xl">
      <h1 class="text-2xl font-bold text-slate-900">Reportar un corte</h1>
      <p class="mt-1 text-sm text-slate-600">
        Solo puede haber un corte abierto por zona. Si tu zona ya tiene uno, vas a poder confirmarlo
        en vez de crear otro.
      </p>

      @if (loading()) {
        <p class="py-10 text-center text-sm text-slate-500">Cargando zonas…</p>
      } @else if (duplicate(); as dup) {
        <!-- El backend respondió 409 reporte_duplicado -->
        <section class="mt-6 rounded-xl border border-amber-300 bg-amber-50 p-6">
          <h2 class="text-lg font-bold text-amber-900">{{ dup.zoneName }} ya tiene un corte abierto</h2>
          <p class="mt-2 text-sm text-amber-900">
            Ya hay un reporte en tu zona y {{ dup.confirmationCount }}
            {{ dup.confirmationCount === 1 ? 'vecino lo confirmó' : 'vecinos lo confirmaron' }}.
            ¿Te afecta a vos también?
          </p>

          <div class="mt-5 flex flex-wrap gap-3">
            @if (!dup.alreadyConfirmedByMe) {
              <button
                type="button"
                [disabled]="saving()"
                (click)="confirmExisting(dup.existingReportId)"
                class="rounded-lg bg-amber-600 px-4 py-2 text-sm font-semibold text-white hover:bg-amber-700 disabled:opacity-60"
              >
                Sí, a mí también me afecta
              </button>
            } @else {
              <span class="self-center text-sm font-medium text-amber-900">
                ✓ Ya habías confirmado este corte.
              </span>
            }

            <a
              [routerLink]="['/reportes', dup.existingReportId]"
              class="rounded-lg border border-amber-400 px-4 py-2 text-sm font-medium text-amber-900 hover:bg-amber-100"
            >
              Ver el reporte
            </a>
            <button
              type="button"
              (click)="duplicate.set(null)"
              class="rounded-lg px-4 py-2 text-sm font-medium text-amber-900 hover:underline"
            >
              Elegir otra zona
            </button>
          </div>
        </section>
      } @else {
        <form
          [formGroup]="form"
          (ngSubmit)="submit()"
          class="mt-6 space-y-4 rounded-xl border border-slate-200 bg-white p-6"
        >
          <label class="block text-sm">
            <span class="mb-1 block font-medium text-slate-700">Zona</span>
            <select formControlName="zoneId" class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm">
              <option value="">Elegí tu zona…</option>
              @for (zone of zones(); track zone.id) {
                <option [value]="zone.id">{{ zone.name }} — {{ zone.sector }}</option>
              }
            </select>
          </label>

          <!-- Aviso antes de mandar la petición: la zona ya trae activeReportId -->
          @if (zoneWithActiveCut(); as zone) {
            <div class="rounded-lg border border-amber-300 bg-amber-50 px-4 py-3 text-sm text-amber-900">
              <strong>{{ zone.name }}</strong> ya tiene un corte abierto.
              <a [routerLink]="['/reportes', zone.activeReportId]" class="font-semibold underline">
                Abrilo y confirmalo
              </a>
              en vez de crear otro.
            </div>
          }

          <label class="block text-sm">
            <span class="mb-1 block font-medium text-slate-700">Dirección aproximada</span>
            <input
              formControlName="address"
              placeholder="Frente al parque, 3ra calle"
              class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
            />
            @if (form.controls.address.touched && form.controls.address.invalid) {
              <span class="mt-1 block text-xs text-red-600">Al menos 5 caracteres.</span>
            }
          </label>

          <label class="block text-sm">
            <span class="mb-1 block font-medium text-slate-700">
              ¿A qué hora se fue la luz? <span class="font-normal text-slate-400">(opcional)</span>
            </span>
            <input
              type="datetime-local"
              formControlName="startedAt"
              class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
            />
            <span class="mt-1 block text-xs text-slate-500">
              Si lo dejás vacío se usa la hora actual. No puede estar en el futuro.
            </span>
          </label>

          <label class="block text-sm">
            <span class="mb-1 block font-medium text-slate-700">
              Enlace a una foto <span class="font-normal text-slate-400">(opcional)</span>
            </span>
            <input
              formControlName="evidenceUrl"
              placeholder="https://…"
              class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
            />
          </label>

          <div class="flex gap-3 pt-2">
            <button
              type="submit"
              [disabled]="saving()"
              class="rounded-lg bg-brand-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-brand-700 disabled:opacity-60"
            >
              {{ saving() ? 'Enviando…' : 'Enviar reporte' }}
            </button>
            <a
              routerLink="/reportes"
              class="rounded-lg border border-slate-300 px-4 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-50"
            >
              Cancelar
            </a>
          </div>
        </form>
      }
    </div>
  `,
})
export class ReportCreateComponent implements OnInit {
  private fb = inject(FormBuilder);
  private zonesService = inject(ZonesService);
  private reportsService = inject(ReportsService);
  private toast = inject(ToastService);
  private router = inject(Router);

  loading = signal(true);
  saving = signal(false);
  zones = signal<Zone[]>([]);
  duplicate = signal<DuplicateReportData | null>(null);

  form = this.fb.nonNullable.group({
    zoneId: ['', Validators.required],
    address: ['', [Validators.required, Validators.minLength(5)]],
    startedAt: [''],
    evidenceUrl: [''],
  });

  ngOnInit(): void {
    this.zonesService.getAll(true).subscribe({
      next: (zones) => {
        this.zones.set(zones);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.error(err.error?.error ?? 'No se pudieron cargar las zonas.');
      },
    });
  }

  // La zona elegida, solo si ya tiene un corte abierto
  zoneWithActiveCut(): Zone | null {
    const zoneId = this.form.controls.zoneId.value;
    const zone = this.zones().find((z) => z.id === zoneId);
    return zone && zone.activeReportId ? zone : null;
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const values = this.form.getRawValue();
    this.saving.set(true);

    this.reportsService
      .create({
        zoneId: values.zoneId,
        address: values.address,
        startedAt: localToIso(values.startedAt),
        evidenceUrl: values.evidenceUrl || undefined,
      })
      .subscribe({
        next: (report) => {
          this.saving.set(false);
          this.toast.success('Reporte enviado.');
          this.router.navigate(['/reportes', report.id]);
        },
        error: (err) => {
          this.saving.set(false);

          // En vez de un error seco, se ofrece confirmar el reporte que ya existe
          if (err.status === 409 && err.error?.code === 'reporte_duplicado') {
            this.duplicate.set(err.error.data);
            return;
          }
          this.toast.error(err.error?.error ?? 'No se pudo crear el reporte.');
        },
      });
  }

  confirmExisting(reportId: string): void {
    this.saving.set(true);
    this.reportsService.confirm(reportId).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Listo, tu confirmación quedó registrada.');
        this.router.navigate(['/reportes', reportId]);
      },
      error: (err) => {
        this.saving.set(false);
        this.toast.error(err.error?.error ?? 'No se pudo confirmar.');
      },
    });
  }
}
