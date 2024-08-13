import { Component, Input, OnInit } from '@angular/core';
import { EventService } from '../services/event.service';
import { UserService } from '../services/user.service';
import { catchError, map, Observable, of } from 'rxjs';
import { EventCommunicationService } from '../services/event-communication.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-event',
  templateUrl: './event.component.html',
  styleUrls: ['./event.component.css']
})
export class EventComponent implements OnInit {
  @Input() event: any;
  lowestTicketPrice: number | undefined;
  emailOfLoggedUser$: Observable<string | null>;

  constructor(
    private eventService: EventService,
    private userService: UserService,
    private router: Router,
    private eventCommunicationService: EventCommunicationService
  ) { 
    this.emailOfLoggedUser$ = this.userService.getCurrentUserEmail();
  }

  ngOnInit(): void {
    this.eventService.getLowestTicketPrice(this.event.id).subscribe(
      price => {
        this.lowestTicketPrice = price;
      },
      error => {
        console.error('Error fetching lowest ticket price:', error);
        this.lowestTicketPrice = undefined; // Set to undefined to show 'N/A' in the UI
      }
    );
  }
  hideAndNavigateTo(path: string) {
    this.eventCommunicationService.hideNavBar();
    this.router.navigate([path]);
  }
  showDetails(): void {
    this.eventCommunicationService.hideNavBar();
    this.router.navigate(['/event-details', this.event.id]);
  }  
  
  voteDown(): void {
    this.emailOfLoggedUser$.subscribe(email => {
      if (email) {
        this.userService.getAccountByEmail(email).subscribe(
          user => {
            const token = localStorage.getItem('token');
            const loginResponse = { token: token || '', userId: user.id };
            this.eventService.addMinus(this.event.id, loginResponse).subscribe(
              updatedEvent => {
                console.log('Minus added and event updated:', updatedEvent);
                this.event = updatedEvent;
                this.eventCommunicationService.updateEventInList(updatedEvent);
              },
              error => {
                console.error('Error adding minus:', error);
              }
            );
          },
          error => {
            console.error('Error fetching user account by email:', error);
          }
        );
      } else {
        this.hideAndNavigateTo('/login');
      }
    });
  }

  hasReportedEvent(userId: number, eventId: number): Observable<boolean> {
    return this.eventService.hasReportedEvent(userId, this.event.id).pipe(
      map(response => response.hasReported),
      catchError(() => of(false)) // Return false in case of error
    );
  }
  
  reportEvent(): void {
    this.emailOfLoggedUser$.subscribe(email => {
      if (email) {
        this.userService.getAccountByEmail(email).subscribe(user => {
          const token = localStorage.getItem('token');
          const loginResponse = { token: token || '', userId: user.id };
  
          this.hasReportedEvent(user.id, this.event.id).subscribe(hasReported => {
            if (hasReported) {
              this.eventService.undoReportEvent(this.event.id, loginResponse).subscribe(
                updatedEvent => {
                  console.log('Event report undone and updated:', updatedEvent);
                  this.event = updatedEvent;
                  this.eventCommunicationService.updateEventInList(updatedEvent);
                },
                error => {
                  console.error('Error undoing event report:', error);
                }
              );
            } else {
              this.eventService.reportEvent(this.event.id, loginResponse).subscribe(
                updatedEvent => {
                  console.log('Event reported and updated:', updatedEvent);
                  this.event = updatedEvent;
                  this.eventCommunicationService.updateEventInList(updatedEvent);
                },
                error => {
                  console.error('Error reporting event:', error);
                }
              );
            }
          });
        });
      } else {
        this.hideAndNavigateTo('/login');
      }
    });
  }
  
  

  voteUp(): void {
    this.emailOfLoggedUser$.subscribe(email => {
        if (email) {
            this.userService.getAccountByEmail(email).subscribe(
                user => {
                    const token = localStorage.getItem('token');
                    const loginResponse = { token: token || '', userId: user.id };
                    this.eventService.addPlus(this.event.id, loginResponse).subscribe(
                        updatedEvent => {
                            console.log('Plus added and event updated:', updatedEvent);
                            this.event = updatedEvent;
                            this.eventCommunicationService.updateEventInList(updatedEvent);
                        },
                        error => {
                            console.error('Error adding plus:', error);
                        }
                    );
                },
                error => {
                    console.error('Error fetching user account by email:', error);
                }
            );
        } else {
          this.hideAndNavigateTo('/login');
        }
    });
}

  
  refreshEvent(): void {
    this.eventService.refreshEvent(this.event.id).subscribe(
      updatedEvent => {
        console.log('Event refreshed:', updatedEvent);
        this.event = updatedEvent;
        this.eventCommunicationService.updateEventInList(updatedEvent);
      },
      error => {
        console.error('Error refreshing event:', error);
      }
    );
  }
  

  getLocationImage(location: number): string {
    switch (location) {
      case 0:
        return 'assets/reallife.png';
      case 1:
        return 'assets/global.png';
      case 2:
        return 'assets/Hybrid.png';
      default:
        return 'assets/reallife.PNG';
    }
  }

  getLocationText(location: number): string {
    switch (location) {
      case 0:
        return 'Na miejscu';
      case 1:
        return 'Online';
      case 2:
        return 'Hybrydowo';
      default:
        return '';
    }
  }

  getImageStyle(event: any): any {
    const aspectRatio = event.image.width / event.image.height;
    if (aspectRatio > 1) {
      return 'landscape-image';
    } else if (aspectRatio < 1) {
      return 'portrait-image';
    } else {
      return 'square-image';
    }
  }


  getLocationImageStyle(location: number): any {
    if (location === 0) {
      return { width: '2vw' };
    } else {
      return { width: '3vw' };
    }
  }

  loadImageError(event: Event): void {
    const target = event.target as HTMLImageElement;
    target.src = 'assets/events/default.PNG';
  }
}
