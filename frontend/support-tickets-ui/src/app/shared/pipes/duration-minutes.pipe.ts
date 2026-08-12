import { Pipe, PipeTransform } from '@angular/core';

/** Presentation only - the API still speaks in raw minutes (90 -> "1h 30m"). */
@Pipe({ name: 'durationMinutes', standalone: true })
export class DurationMinutesPipe implements PipeTransform {
  transform(totalMinutes: number | null | undefined): string {
    if (!totalMinutes) return '0m';

    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    if (hours === 0) return `${minutes}m`;
    if (minutes === 0) return `${hours}h`;
    return `${hours}h ${minutes}m`;
  }
}
