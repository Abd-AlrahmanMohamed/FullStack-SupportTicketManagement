export interface AgentWorkload {
  agentName: string;
  activeTickets: number;
}

export interface Dashboard {
  totalTickets: number;
  openTickets: number;
  inProgressTickets: number;
  resolvedTickets: number;
  closedTickets: number;
  openCriticalTickets: number;
  averageResolutionTimeHours: number;
  agentWorkload: AgentWorkload[];
}
