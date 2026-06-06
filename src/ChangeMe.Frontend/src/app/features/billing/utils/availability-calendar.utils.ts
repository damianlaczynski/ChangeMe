import {
  AvailabilityEntryDto,
  AvailabilityEntrySource,
  AvailabilityStatus
} from '@features/billing/models/availability.model';
import { parseIsoDateString, toIsoDateString } from '@features/time/utils/time.utils';

export function getSourcePriority(source: AvailabilityEntrySource): number {
  switch (source) {
    case AvailabilityEntrySource.Leave:
      return 0;
    case AvailabilityEntrySource.Manual:
      return 1;
    case AvailabilityEntrySource.Recurring:
      return 2;
    default:
      return 3;
  }
}

export function getEntriesForUserDay(
  entries: AvailabilityEntryDto[],
  userId: string,
  date: string
): AvailabilityEntryDto[] {
  return entries
    .filter(
      (entry) =>
        entry.userId === userId && isDateInRange(date, entry.startDate, entry.endDate)
    )
    .sort((a, b) => {
      const priority = getSourcePriority(a.source) - getSourcePriority(b.source);
      if (priority !== 0) {
        return priority;
      }

      if (a.allDay !== b.allDay) {
        return a.allDay ? -1 : 1;
      }

      return (a.startTime ?? '').localeCompare(b.startTime ?? '');
    });
}

export function resolveEffectiveEntry(
  entries: AvailabilityEntryDto[],
  userId: string,
  date: string
): AvailabilityEntryDto | null {
  return getEntriesForUserDay(entries, userId, date)[0] ?? null;
}

export function countDayEntries(
  entries: AvailabilityEntryDto[],
  userId: string,
  date: string
): number {
  return getEntriesForUserDay(entries, userId, date).length;
}

export function isDateInRange(
  date: string,
  startDate: string,
  endDate: string
): boolean {
  return date >= startDate && date <= endDate;
}

export function isWeekendDate(date: string): boolean {
  const day = parseIsoDateString(date).getDay();
  return day === 0 || day === 6;
}

export function isTodayDate(date: string): boolean {
  return date === toIsoDateString(new Date());
}

export function formatAvailabilityTimeRange(entry: AvailabilityEntryDto): string {
  if (entry.allDay) {
    return 'All day';
  }

  return `${entry.startTime ?? ''}–${entry.endTime ?? ''}`;
}

export function truncateAvailabilityNotes(value: string, maxLength = 20): string {
  const trimmed = value.trim();
  if (trimmed.length <= maxLength) {
    return trimmed;
  }

  return `${trimmed.slice(0, maxLength)}…`;
}

export function getAvailabilitySourceLabel(source: AvailabilityEntrySource): string {
  switch (source) {
    case AvailabilityEntrySource.Leave:
      return 'Leave';
    case AvailabilityEntrySource.Manual:
      return 'Manual';
    case AvailabilityEntrySource.Recurring:
      return 'Recurring';
    default:
      return source;
  }
}

export function getAvailabilityEntryTooltip(entry: AvailabilityEntryDto): string {
  const lines = [
    `Source: ${getAvailabilitySourceLabel(entry.source)}`,
    `Status: ${getAvailabilityStatusLabel(entry)}`,
    entry.allDay ? 'All day' : formatAvailabilityTimeRange(entry)
  ];

  const notes = entry.notes?.trim();
  if (notes) {
    lines.push(`Notes: ${notes}`);
  }

  return lines.join('\n');
}

export function getMonthGridDates(year: number, month: number): string[] {
  const firstDay = new Date(year, month, 1);
  const lastDay = new Date(year, month + 1, 0);
  const startOffset = (firstDay.getDay() + 6) % 7;
  const gridStart = new Date(year, month, 1 - startOffset);

  const dates: string[] = [];
  for (let index = 0; index < 42; index += 1) {
    const current = new Date(gridStart);
    current.setDate(gridStart.getDate() + index);
    dates.push(toIsoDateString(current));
    if (current >= lastDay && (index + 1) % 7 === 0 && current.getMonth() !== month) {
      break;
    }
  }

  return dates;
}

export function getWeekDates(anchor: Date): string[] {
  const mondayOffset = (anchor.getDay() + 6) % 7;
  const monday = new Date(anchor);
  monday.setDate(anchor.getDate() - mondayOffset);

  return Array.from({ length: 7 }, (_, index) => {
    const date = new Date(monday);
    date.setDate(monday.getDate() + index);
    return toIsoDateString(date);
  });
}

export function shiftMonth(date: Date, delta: number): Date {
  return new Date(date.getFullYear(), date.getMonth() + delta, 1);
}

export function shiftWeek(date: Date, deltaWeeks: number): Date {
  const next = new Date(date);
  next.setDate(next.getDate() + deltaWeeks * 7);
  return next;
}

export function getCalendarRange(
  view: 'month' | 'week',
  anchor: Date
): { from: string; to: string } {
  if (view === 'week') {
    const dates = getWeekDates(anchor);
    return { from: dates[0], to: dates[dates.length - 1] };
  }

  const from = new Date(anchor.getFullYear(), anchor.getMonth(), 1);
  const to = new Date(anchor.getFullYear(), anchor.getMonth() + 1, 0);
  return { from: toIsoDateString(from), to: toIsoDateString(to) };
}

export function isInMonth(date: string, anchor: Date): boolean {
  const parsed = parseIsoDateString(date);
  return (
    parsed.getMonth() === anchor.getMonth() &&
    parsed.getFullYear() === anchor.getFullYear()
  );
}

export function getAvailabilityStatusSeverity(
  entry: AvailabilityEntryDto
): 'success' | 'danger' | 'info' | 'contrast' | 'secondary' {
  if (entry.source === AvailabilityEntrySource.Leave) {
    return 'danger';
  }

  switch (entry.status) {
    case AvailabilityStatus.Available:
      return 'success';
    case AvailabilityStatus.Unavailable:
      return 'danger';
    case AvailabilityStatus.Remote:
      return 'info';
    case AvailabilityStatus.OnSite:
      return 'contrast';
    default:
      return 'secondary';
  }
}

export function getAvailabilityStatusLabel(entry: AvailabilityEntryDto): string {
  if (entry.source === AvailabilityEntrySource.Leave) {
    return 'Leave';
  }

  switch (entry.status) {
    case AvailabilityStatus.Available:
      return 'Available';
    case AvailabilityStatus.Unavailable:
      return 'Unavailable';
    case AvailabilityStatus.Remote:
      return 'Remote';
    case AvailabilityStatus.OnSite:
      return 'On-site';
    default:
      return entry.status;
  }
}

export function isEffectiveInStatus(
  entry: AvailabilityEntryDto,
  statuses: string[]
): boolean {
  if (statuses.length === 0) {
    return true;
  }

  if (entry.source === AvailabilityEntrySource.Leave) {
    return statuses.includes('Leave');
  }

  return statuses.includes(entry.status);
}

export function countAvailableDays(
  entries: AvailabilityEntryDto[],
  userId: string,
  weekDates: string[]
): number {
  return weekDates.filter((date) => {
    const effective = resolveEffectiveEntry(entries, userId, date);
    if (!effective) {
      return false;
    }

    if (effective.source === AvailabilityEntrySource.Leave) {
      return false;
    }

    return effective.status !== AvailabilityStatus.Unavailable;
  }).length;
}

export function countTeamDaySummary(
  entries: AvailabilityEntryDto[],
  users: { id: string }[],
  date: string
): { availableCount: number; awayCount: number } {
  let availableCount = 0;
  let awayCount = 0;

  for (const user of users) {
    const effective = resolveEffectiveEntry(entries, user.id, date);
    if (!effective) {
      continue;
    }

    if (
      effective.source === AvailabilityEntrySource.Leave ||
      effective.status === AvailabilityStatus.Unavailable
    ) {
      awayCount += 1;
    } else {
      availableCount += 1;
    }
  }

  return { availableCount, awayCount };
}

export function buildHalfHourSlots(startHour: number, endHour: number): string[] {
  const slots: string[] = [];
  for (let hour = startHour; hour <= endHour; hour += 1) {
    slots.push(`${String(hour).padStart(2, '0')}:00`);
    if (hour < endHour) {
      slots.push(`${String(hour).padStart(2, '0')}:30`);
    }
  }

  return slots;
}

export const WEEK_GRID_SLOT_HEIGHT_PX = 24;

export function getWeekGridBounds(
  settings: {
    defaultWorkdayStart: string;
    defaultWorkdayEnd: string;
  } | null
): {
  startMinutes: number;
  endMinutes: number;
  slots: string[];
} {
  if (!settings) {
    return {
      startMinutes: 6 * 60,
      endMinutes: 20 * 60,
      slots: buildHalfHourSlots(6, 20)
    };
  }

  const workStart = parseTimeToMinutes(settings.defaultWorkdayStart);
  const workEnd = parseTimeToMinutes(settings.defaultWorkdayEnd);
  const startMinutes = Math.max(0, workStart - 60);
  const endMinutes = Math.min(24 * 60, workEnd + 60);
  const startHour = Math.floor(startMinutes / 60);
  const endHour = Math.ceil(endMinutes / 60);

  return {
    startMinutes,
    endMinutes,
    slots: buildHalfHourSlots(startHour, endHour)
  };
}

export function getAllDayEntriesForDay(
  entries: AvailabilityEntryDto[],
  userId: string,
  date: string
): AvailabilityEntryDto[] {
  return getEntriesForUserDay(entries, userId, date).filter((entry) => entry.allDay);
}

export function getTimedEntriesForDay(
  entries: AvailabilityEntryDto[],
  userId: string,
  date: string
): AvailabilityEntryDto[] {
  return getEntriesForUserDay(entries, userId, date).filter(
    (entry) => !entry.allDay && entry.startTime && entry.endTime
  );
}

export function getEntryBlockStyle(
  entry: AvailabilityEntryDto,
  gridStartMinutes: number,
  gridEndMinutes: number
): { top: string; height: string } {
  if (!entry.startTime || !entry.endTime) {
    return { top: '0', height: '0' };
  }

  const start = parseTimeToMinutes(entry.startTime);
  const end = parseTimeToMinutes(entry.endTime);
  const gridSpan = Math.max(gridEndMinutes - gridStartMinutes, 30);
  const topPct =
    ((Math.max(start, gridStartMinutes) - gridStartMinutes) / gridSpan) * 100;
  const heightPct =
    ((Math.min(end, gridEndMinutes) - Math.max(start, gridStartMinutes)) / gridSpan) *
    100;

  return {
    top: `${topPct}%`,
    height: `${Math.max(heightPct, 4)}%`
  };
}

export function getEntryBlockClass(entry: AvailabilityEntryDto): string {
  if (entry.source === AvailabilityEntrySource.Leave) {
    return 'bg-red-100 text-red-900 dark:bg-red-950 dark:text-red-100';
  }

  switch (entry.status) {
    case AvailabilityStatus.Available:
      return 'bg-green-100 text-green-900 dark:bg-green-950 dark:text-green-100';
    case AvailabilityStatus.Unavailable:
      return 'bg-red-100 text-red-900 dark:bg-red-950 dark:text-red-100';
    case AvailabilityStatus.Remote:
      return 'bg-sky-100 text-sky-900 dark:bg-sky-950 dark:text-sky-100';
    case AvailabilityStatus.OnSite:
      return 'bg-primary-100 text-primary-900 dark:bg-primary-950 dark:text-primary-100';
    default:
      return 'bg-surface-100 text-surface-900 dark:bg-surface-800 dark:text-surface-100';
  }
}

export function parseTimeToMinutes(value: string): number {
  const [hours, minutes] = value.split(':').map(Number);
  return hours * 60 + minutes;
}

export function entryCoversSlot(entry: AvailabilityEntryDto, slot: string): boolean {
  if (entry.allDay) {
    return true;
  }

  if (!entry.startTime || !entry.endTime) {
    return false;
  }

  const slotMinutes = parseTimeToMinutes(slot);
  const startMinutes = parseTimeToMinutes(entry.startTime);
  const endMinutes = parseTimeToMinutes(entry.endTime);
  return slotMinutes >= startMinutes && slotMinutes < endMinutes;
}
