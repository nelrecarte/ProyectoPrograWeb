import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="p-6 bg-gray-50 min-h-screen">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-3xl font-bold text-gray-800">Tablero de Administración</h1>
        
        <!-- Filtro por Zona -->
        <div class="flex items-center space-x-2">
          <label class="font-medium text-gray-600">Zona:</label>
          <select 
            [(ngModel)]="selectedZone" 
            (change)="fetchStatistics()" 
            class="border rounded p-2 bg-white shadow-sm">
            <option value="">Todas las zonas</option>
            <option value="1">Zona Norte</option>
            <option value="2">Zona Sur</option>
            <option value="3">Zona Centro</option>
          </select>
        </div>
      </div>

      <!-- 1. Cuatro métricas + Promedio de Resolución -->
      <div class="grid grid-cols-1 md:grid-cols-5 gap-4 mb-8">
        <div class="p-4 bg-white rounded-lg shadow border-t-4 border-blue-500">
          <p class="text-xs text-gray-500 font-bold uppercase">Reportes Totales</p>
          <p class="text-2xl font-bold mt-1">{{ stats?.totalReports || 0 }}</p>
        </div>
        <div class="p-4 bg-white rounded-lg shadow border-t-4 border-yellow-500">
          <p class="text-xs text-gray-500 font-bold uppercase">Activos</p>
          <p class="text-2xl font-bold mt-1">{{ stats?.activeReports || 0 }}</p>
        </div>
        <div class="p-4 bg-white rounded-lg shadow border-t-4 border-green-500">
          <p class="text-xs text-gray-500 font-bold uppercase">Resueltos</p>
          <p class="text-2xl font-bold mt-1">{{ stats?.resolvedReports || 0 }}</p>
        </div>
        <div class="p-4 bg-white rounded-lg shadow border-t-4 border-red-500">
          <p class="text-xs text-gray-500 font-bold uppercase">Sin Verificar</p>
          <p class="text-2xl font-bold mt-1">{{ stats?.unverifiedReports || 0 }}</p>
        </div>
        <div class="p-4 bg-white rounded-lg shadow border-t-4 border-purple-500">
          <p class="text-xs text-gray-500 font-bold uppercase">Prom. Resolución</p>
          <p class="text-2xl font-bold mt-1">{{ stats?.avgResolutionTime || '0h' }}</p>
        </div>
      </div>

      <!-- Sección de Gráficos en SVG -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
        <!-- Gráfico 1: Barras de Cortes por Zona -->
        <div class="bg-white p-5 rounded-lg shadow">
          <h2 class="text-lg font-bold text-gray-700 mb-4">Cortes por Zona</h2>
          <svg viewBox="0 0 300 150" class="w-full h-44">
            <line x1="30" y1="130" x2="280" y2="130" stroke="#e2e8f0" stroke-width="2"/>
            <rect x="50" y="40" width="35" height="90" fill="#3b82f6" rx="3"/>
            <rect x="110" y="70" width="35" height="60" fill="#3b82f6" rx="3"/>
            <rect x="170" y="20" width="35" height="110" fill="#3b82f6" rx="3"/>
            <rect x="230" y="90" width="35" height="40" fill="#3b82f6" rx="3"/>
          </svg>
        </div>

        <!-- Gráfico 2: Circular por Estado (Mantiene los 4 estados fija la estructura) -->
        <div class="bg-white p-5 rounded-lg shadow">
          <h2 class="text-lg font-bold text-gray-700 mb-4">Estado de Reportes</h2>
          <div class="flex items-center justify-around">
            <svg viewBox="0 0 36 36" class="w-36 h-36">
              <path stroke-dasharray="25, 100" stroke="#eab308" stroke-width="3.8" fill="none" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
              <path stroke-dasharray="35, 100" stroke-dashoffset="-25" stroke="#22c55e" stroke-width="3.8" fill="none" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
              <path stroke-dasharray="20, 100" stroke-dashoffset="-60" stroke="#ef4444" stroke-width="3.8" fill="none" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
              <path stroke-dasharray="20, 100" stroke-dashoffset="-80" stroke="#3b82f6" stroke-width="3.8" fill="none" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
            </svg>
            <div class="text-sm space-y-1">
              <div class="flex items-center"><span class="w-3 h-3 bg-yellow-500 rounded-full mr-2"></span> Activo</div>
              <div class="flex items-center"><span class="w-3 h-3 bg-green-500 rounded-full mr-2"></span> Resuelto</div>
              <div class="flex items-center"><span class="w-3 h-3 bg-red-500 rounded-full mr-2"></span> Sin Verificar</div>
              <div class="flex items-center"><span class="w-3 h-3 bg-blue-500 rounded-full mr-2"></span> Cerrado</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Tabla de Rendimiento por Técnico -->
      <div class="bg-white rounded-lg shadow overflow-hidden">
        <h2 class="text-lg font-bold text-gray-700 p-5 border-b">Rendimiento por Técnico</h2>
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-gray-100 text-gray-600 text-xs uppercase font-semibold">
              <th class="p-4">Técnico</th>
              <th class="p-4">Asignados</th>
              <th class="p-4">Resueltos</th>
              <th class="p-4">Efectividad</th>
            </tr>
          </thead>
          <tbody class="divide-y text-sm">
            <tr *ngFor="let tech of stats?.techniciansPerformance || []">
              <td class="p-4 font-medium">{{ tech.name }}</td>
              <td class="p-4">{{ tech.assigned }}</td>
              <td class="p-4">{{ tech.resolved }}</td>
              <td class="p-4">{{ tech.efficiency }}%</td>
            </tr>
            <tr *ngIf="!stats?.techniciansPerformance?.length">
              <td colspan="4" class="p-4 text-center text-gray-400">Sin datos de técnicos disponibles</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `
})
export class DashboardComponent implements OnInit {
  private http = inject(HttpClient);
  stats: any = null;
  selectedZone: string = '';

  ngOnInit(): void {
    this.fetchStatistics();
  }

  fetchStatistics(): void {
    const url = this.selectedZone 
      ? `/api/statistics?zoneId=${this.selectedZone}`
      : '/api/statistics';

    this.http.get(url).subscribe({
      next: (data) => (this.stats = data),
      error: (err) => console.error('Error cargando estadísticas:', err)
    });
  }
}