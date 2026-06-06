import {
  AvailabilityEntrySource,
  AvailabilityStatus
} from '@features/billing/models/availability.model';
import { countTeamDaySummary } from '@features/billing/utils/availability-calendar.utils';
import { describe, expect, it } from 'vitest';

describe('countTeamDaySummary', () => {
  const date = '2026-08-15';
  const userA = 'user-a';
  const userB = 'user-b';
  const userC = 'user-c';

  it('counts available and away users for a day', () => {
    const entries = [
      {
        id: '1',
        userId: userA,
        startDate: date,
        endDate: date,
        allDay: true,
        status: AvailabilityStatus.Remote,
        notes: '',
        source: AvailabilityEntrySource.Manual,
        createdAt: '2026-08-01T10:00:00Z'
      },
      {
        id: '2',
        userId: userB,
        startDate: date,
        endDate: date,
        allDay: true,
        status: AvailabilityStatus.Unavailable,
        notes: '',
        source: AvailabilityEntrySource.Manual,
        createdAt: '2026-08-01T10:00:00Z'
      },
      {
        id: '3',
        userId: userC,
        startDate: date,
        endDate: date,
        allDay: true,
        status: AvailabilityStatus.Unavailable,
        notes: '',
        source: AvailabilityEntrySource.Leave,
        createdAt: '2026-08-01T10:00:00Z'
      }
    ];

    expect(
      countTeamDaySummary(entries, [{ id: userA }, { id: userB }, { id: userC }], date)
    ).toEqual({
      availableCount: 1,
      awayCount: 2
    });
  });

  it('ignores users without entries on the day', () => {
    const entries = [
      {
        id: '1',
        userId: userA,
        startDate: date,
        endDate: date,
        allDay: true,
        status: AvailabilityStatus.OnSite,
        notes: '',
        source: AvailabilityEntrySource.Recurring,
        createdAt: '2026-08-01T10:00:00Z'
      }
    ];

    expect(countTeamDaySummary(entries, [{ id: userA }, { id: userB }], date)).toEqual({
      availableCount: 1,
      awayCount: 0
    });
  });
});
