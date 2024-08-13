import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { EventService } from '../services/event.service';
import { EventCommunicationService } from '../services/event-communication.service';
import { Subscription } from 'rxjs';
import { UserService } from '../services/user.service';

@Component({
  selector: 'app-baner',
  templateUrl: './baner.component.html',
  styleUrls: ['./baner.component.css']
})
export class BanerComponent {
  searchTerm: string = '';
  userEmail: string = '';
  searchPlaceholder: string = 'Wyszukaj wydarzenia...'; 
  isLoggedIn: boolean = false;
  currentUserSubscription: Subscription | undefined;
  currentUserImageSubscription: Subscription | undefined;
  email: string = '';
  userImage: string = '';
  user: string = '';
  currentSort: string; 

  constructor(
    private router: Router,
    private userService: UserService,
    private eventService: EventService,
    private eventCommunicationService: EventCommunicationService) {
    }
    
    ngOnInit(): void {
      this.currentUserSubscription = this.userService.currentUserEmail$.subscribe(email => {
        this.userEmail = email || '';
        this.isLoggedIn = !!email;
      });
      this.eventCommunicationService.currentSort$.subscribe(sortType => {
        this.currentSort = sortType;
      });
      this.currentUserImageSubscription = this.userService.currentUserImage$.subscribe(imageUrl => {
        this.userImage = imageUrl || 'assets/users/default.PNG';
      });
    }
    ngOnDestroy(): void {
      this.currentUserSubscription?.unsubscribe();
      this.currentUserImageSubscription?.unsubscribe();
    }
  hideAndNavigateTo(path: string) {
    this.eventCommunicationService.hideNavBar();
    this.router.navigate([path]);
  }

  navigateTo(path: string) {
    this.resetSearchTerm();
    this.eventCommunicationService.showNavBar();
    this.router.navigate([path]);
  }

  showLoginForm = false;
  toggleLoginForm() {
    this.showLoginForm = !this.showLoginForm;
  }

  searchEvents(): void {
    this.eventCommunicationService.requestEventListRefresh(
      () => this.eventService.searchEvents(this.searchTerm, this.currentSort),
      this.currentSort
    );
  }
  
  onRegister() {
    this.router.navigate(['/register']);
  }

  onLogout() {
    localStorage.removeItem('authToken');
    localStorage.removeItem('userId');
    this.isLoggedIn = false;
    this.userEmail = '';
    this.router.navigate(['/events']);
  }
  resetSearchTerm() {
    this.searchTerm = '';
    this.searchPlaceholder = 'Wyszukaj wydarzenia...'; 
  }

  loadImageError(event: Event): void {
    const target = event.target as HTMLImageElement;
    target.src = 'assets/users/default.PNG';
  }
}
