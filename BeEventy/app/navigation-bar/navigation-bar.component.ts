import { Component } from '@angular/core';
import { EventService } from '../services/event.service';
import { EventCommunicationService } from '../services/event-communication.service';

@Component({
  selector: 'app-navigation-bar',
  templateUrl: './navigation-bar.component.html',
  styleUrls: ['./navigation-bar.component.css']
})
export class NavigationBarComponent {
  currentSort: string;

  constructor(
    private eventService: EventService,
    private eventCommunicationService: EventCommunicationService
  ) {
    this.eventCommunicationService.currentSort$.subscribe(sortType => {
      this.currentSort = sortType;
    });
  }

  sortByDate(): void {
    this.eventCommunicationService.requestEventListRefresh(() => this.eventService.getSortedEventsByDate(), 'date');
  }

  sortByVotes(): void {
    this.eventCommunicationService.requestEventListRefresh(() => this.eventService.getSortedEventsByVotes(), 'votes');
  }

  sortByClosestDate(): void {
    this.eventCommunicationService.requestEventListRefresh(() => this.eventService.getSortedEventsByClosestDate(), 'closest');
  }
}
