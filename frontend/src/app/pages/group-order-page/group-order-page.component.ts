import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { forkJoin, switchMap } from 'rxjs';

import { ParticipantCardComponent } from '@app/components/group-orders/participant-card/participant-card.component';
import { GroupOrdersService } from '@app/services/group-orders/group-orders.service';
import { GuestInvite } from '@app/models/reservations/guest-invite.model';
import { Participant } from '@app/models/group-orders/participant.model';
import { ToastService } from '@app/services/toast/toast.service';
import { ModalComponent } from '@app/shared/custom-components/modal/modal.component';

@Component({
  selector: 'app-group-order-page',
  imports: [
  ParticipantCardComponent,
  ModalComponent,
  CurrencyPipe,
  DatePipe,
  ],
  templateUrl: './group-order-page.component.html',
  styleUrl: './group-order-page.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class GroupOrderPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly groupOrdersService = inject(GroupOrdersService);

  readonly reservationId =
    this.route.snapshot.paramMap.get('reservationId') ?? '';

  readonly invite = signal<GuestInvite | null>(null);
  readonly participants = signal<Participant[]>([]);
  readonly ownerSessionId = signal('');

  readonly loading = signal(true);
  readonly error = signal('');
  readonly groupStatus = signal('Open');

  readonly grandTotal = computed(() =>
    this.participants()
      .filter((person) => person.isSubmitted)
      .reduce((sum, person) => sum + person.total, 0)
  );

  readonly pendingCount = computed(() =>
    this.participants().filter((person) => !person.isSubmitted).length
  );

  readonly hasDishes = computed(() =>
    this.participants().some((person) => person.items.length > 0)
  );

  readonly consolidatedItems = computed(() => {
    const dishes = new Map<string, {
      name: string;
      quantity: number;
    }>();

    for (const person of this.participants()) {
      if (!person.isSubmitted) continue;

      for (const item of person.items) {
        const existing = dishes.get(item.dish.id);

        if (existing) {
          existing.quantity += item.quantity;
        } else {
          dishes.set(item.dish.id, {
            name: item.dish.name,
            quantity: item.quantity,
          });
        }
      }
    }

    return [...dishes.values()];
  });

  ngOnInit(): void {
    this.loadGroup();
  }

  loadGroup(): void {
    this.loading.set(true);
    this.error.set('');

    this.groupOrdersService.getGroupInvite$(this.reservationId)
      .pipe(
        switchMap((invite) => {
          this.invite.set(invite);

          return forkJoin({
            sessions: this.groupOrdersService.getSessions$(
              invite.GroupReservationId
            ),
            ownerSessionId: this.groupOrdersService.getOwnerSessionId$(
              this.reservationId
            ),
            status: this.groupOrdersService.getStatus$(
              invite.GroupReservationId
            ),
          });
        })
      )
      .subscribe({
        next: ({ sessions, ownerSessionId, status }) => {
          this.participants.set(sessions.participants);
          this.ownerSessionId.set(ownerSessionId);
          this.groupStatus.set(status.status);
          this.loading.set(false);
        },
        error: (error) => {
          this.loading.set(false);
          this.error.set(
            error.status === 404
              ? 'Group reservation not found'
              : 'Failed to load group pre-order'
          );
        },
      });
  }

  
private readonly toastService = inject(ToastService);

readonly showSubmitModal = signal(false);
readonly isSubmitting = signal(false);
readonly isSubmitted = signal(false);

readonly canSubmit = computed(() =>
  !this.loading() &&
  !this.error() &&
  !this.isSubmitted() &&
  !this.isSubmitting() &&
  this.participants().some(
    (person) => person.isSubmitted && person.items.length > 0
  )
);

openSubmitModal(): void {
  if (!this.canSubmit()) return;

  this.showSubmitModal.set(true);
}

closeSubmitModal(): void {
  if (this.isSubmitting()) return;

  this.showSubmitModal.set(false);
}

confirmSubmit(): void {
  if (!this.canSubmit()) return;

  const groupReservationId = this.invite()?.GroupReservationId;

  if (!groupReservationId) return;

  this.isSubmitting.set(true);

  this.groupOrdersService
    .submit$(groupReservationId, this.reservationId)
    .subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.isSubmitted.set(true);
        this.groupStatus.set('Submitted');
        this.showSubmitModal.set(false);

        this.toastService.success(
          'Group pre-order sent to restaurant'
        );
      },
      error: () => {
        this.isSubmitting.set(false);
        this.showSubmitModal.set(false);
      },
    });
}

}