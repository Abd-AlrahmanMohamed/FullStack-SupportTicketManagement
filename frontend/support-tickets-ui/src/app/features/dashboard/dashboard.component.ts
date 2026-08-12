import { Component, OnInit, computed, signal } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { DashboardService } from '../../core/services/dashboard.service';
import { Dashboard } from '../../core/models/dashboard.model';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { AvatarComponent } from '../../shared/components/avatar/avatar.component';

const CHART_COLORS = ['#2952cc', '#f59e0b', '#1a7d43', '#9aa0ac'];

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    BaseChartDirective,
    MatCardModule,
    MatIconModule,
    PageHeaderComponent,
    LoadingStateComponent,
    AvatarComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  dashboard = signal<Dashboard | null>(null);
  loading = signal(true);

  maxAgentTickets = computed(() => {
    const workload = this.dashboard()?.agentWorkload ?? [];
    return Math.max(1, ...workload.map((a) => a.activeTickets));
  });

  kpiCards = computed(() => {
    const d = this.dashboard();
    if (!d) return [];

    return [
      { label: 'Total Tickets', value: d.totalTickets, icon: 'confirmation_number' },
      { label: 'Open', value: d.openTickets, icon: 'radio_button_unchecked' },
      { label: 'In Progress', value: d.inProgressTickets, icon: 'autorenew' },
      { label: 'Resolved', value: d.resolvedTickets, icon: 'task_alt' },
      { label: 'Closed', value: d.closedTickets, icon: 'archive' },
      { label: 'Open Critical', value: d.openCriticalTickets, icon: 'warning_amber', accent: 'danger' },
      { label: 'Avg. Resolution', value: `${d.averageResolutionTimeHours}h`, icon: 'schedule' }
    ];
  });

  statusChartData: ChartConfiguration<'doughnut'>['data'] = {
    labels: ['Open', 'In Progress', 'Resolved', 'Closed'],
    datasets: [{ data: [], backgroundColor: CHART_COLORS, borderWidth: 0 }]
  };

  statusChartOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '68%',
    plugins: {
      legend: {
        position: 'bottom',
        labels: { boxWidth: 10, boxHeight: 10, usePointStyle: true, pointStyle: 'circle' }
      }
    }
  };

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.dashboardService.getDashboard().subscribe({
      next: (dashboard) => {
        this.dashboard.set(dashboard);
        this.statusChartData = {
          ...this.statusChartData,
          datasets: [
            {
              ...this.statusChartData.datasets[0],
              data: [
                dashboard.openTickets,
                dashboard.inProgressTickets,
                dashboard.resolvedTickets,
                dashboard.closedTickets
              ]
            }
          ]
        };
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
