import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, switchMap } from 'rxjs';

import { API_BASE_URL } from '@app/shared/config/api-base-url.token';
import { GuestInvite } from '@app/models/reservations/guest-invite.model';
import { GroupSessionsResponse } from '@app/models/group-orders/group-sessions-response.model';

export interface GroupOrderStatus {
  currentParticipants: number;
  maxCapacity: number;
  status: string;
}

@Injectable({
  providedIn: 'root',
})
export class GroupOrdersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getGroupInvite$(reservationId: string): Observable<GuestInvite> {
    return this.http
      .get<{ token: string }>(
        `${this.baseUrl}/reservations/${reservationId}/invite-token`
      )
      .pipe(
        switchMap(({ token }) =>
          this.http.get<GuestInvite>(
            `${this.baseUrl}/guest/invites/${token}`
          )
        )
      );
  }

  getSessions$(
    groupReservationId: string
  ): Observable<GroupSessionsResponse> {
    return this.http.get<GroupSessionsResponse>(
      `${this.baseUrl}/guest/groups/${groupReservationId}/sessions`
    );
  }

  getOwnerSessionId$(reservationId: string): Observable<string> {
    return this.http
      .get<{ guestSessionId: string }>(
        `${this.baseUrl}/reservations/${reservationId}/guest-session-id`
      )
      .pipe(map((response) => response.guestSessionId));
  }

  getStatus$(groupReservationId: string): Observable<GroupOrderStatus> {
    return this.http.get<GroupOrderStatus>(
      `${this.baseUrl}/guest/groups/${groupReservationId}/status`
    );
  }

  submit$(
    groupReservationId: string,
    reservationId: string
  ): Observable<unknown> {
    return this.http.post<unknown>(
      `${this.baseUrl}/group-reservations/${groupReservationId}/submit`,
      { reservationId }
    );
  }
}