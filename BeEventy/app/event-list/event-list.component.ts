import { Component, OnInit, OnDestroy } from '@angular/core';
import { EventService } from '../services/event.service';
import { EventCommunicationService } from '../services/event-communication.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-event-list',
  templateUrl: './event-list.component.html',
  styleUrls: ['./event-list.component.css']
})
export class EventListComponent implements OnInit, OnDestroy {
  events: any[] = [];
  currentPage: number = 1;
  itemsPerPage: number = 6;
  totalPages: number = 0;
  currentSort: string; 
  private eventListSubscription: Subscription | undefined;

  constructor(
    private eventService: EventService,
    private eventCommunicationService: EventCommunicationService
  ) {}

  ngOnInit() {
    this.loadEvents();
    this.eventCommunicationService.currentSort$.subscribe(sortType => {
      this.currentSort = sortType;
    });
    this.eventListSubscription = this.eventCommunicationService.refreshEventList$.subscribe(sortMethod => {
      sortMethod().subscribe(data => {
        this.events = data;
        this.totalPages = Math.ceil(this.events.length / this.itemsPerPage);
        this.currentPage = 1; // Reset to the first page on sorting
      });
    });
    this.eventListSubscription.add(
      this.eventCommunicationService.updateEventInList$.subscribe(updatedEvent => {
        this.updateEventInList(updatedEvent);
      })
    );
  }
  updateEventInList(updatedEvent: any) {
    console.log('updateEventInList called with:', updatedEvent);
    const index = this.events.findIndex(event => event.id === updatedEvent.id);
    if (index !== -1) {
      this.events[index] = updatedEvent;
      console.log('Event list updated:', this.events);
    }
  }
  
  ngOnDestroy() {
    if (this.eventListSubscription) {
      this.eventListSubscription.unsubscribe();
    }
  }

  loadEvents() {
    this.eventService.getAllEvents().subscribe(data => {
      this.events = data;
      this.totalPages = Math.ceil(this.events.length / this.itemsPerPage);
    });
  }

  get paginatedEvents() {
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.events.slice(startIndex, endIndex);
  }

  changePage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }
}
