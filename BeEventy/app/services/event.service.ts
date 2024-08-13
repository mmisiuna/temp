import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class EventService {
  private apiUrl = 'http://localhost:5260/api/event';
  private ticketUrl = 'http://localhost:5260/api/ticket';

  constructor(private http: HttpClient) { }

  getAllEvents(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/getAllValidEvents`);
  }

  getLowestTicketPrice(eventId: number): Observable<number> {
    return this.http.get<number>(`${this.ticketUrl}/lowest-price/event/${eventId}`);
  }

  getSortedEventsByDate(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/sort/date`);
  }

  getSortedEventsByVotes(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/sort/votes`);
  }

  getSortedEventsByClosestDate(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/sort/closest`);
  }

  searchEvents(searchTerm: string, sortType: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/search`, { params: { searchTerm, sortType } });
  }
  

  getEventById(eventId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${eventId}`);
  }
  
  addPlus(eventId: number, loginResponse: { token: string, userId: number }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${eventId}/plus`, loginResponse).pipe(
        tap(response => {
            console.log('Response from server:', response);
        })
    );
}

addMinus(eventId: number, loginResponse: { token: string, userId: number }): Observable<any> {
  return this.http.post<any>(`${this.apiUrl}/${eventId}/minus`, loginResponse).pipe(
    tap(response => {
      console.log('Response from server:', response);
    })
  );
}

reportEvent(eventId: number, loginResponse: { token: string, userId: number }): Observable<any> {
  return this.http.post<any>(`${this.apiUrl}/${eventId}/report`, loginResponse).pipe(
      tap(response => {
          console.log('Response from server:', response);
      })
  );
}

undoReportEvent(eventId: number, loginResponse: { token: string, userId: number }): Observable<any> {
  return this.http.post<any>(`${this.apiUrl}/${eventId}/undo-report`, loginResponse).pipe(
      tap(response => {
          console.log('Response from server:', response);
      })
  );
}

hasReportedEvent(eventId: number, loginResponse: { token: string, userId: number }): Observable<{ hasReported: boolean }> {
  return this.http.get<{ hasReported: boolean }>(`${this.apiUrl}/${eventId}/has-reported`, { params: { userId: loginResponse.userId.toString() } });
}


refreshEvent(eventId: number): Observable<any> {
  console.log('refreshEvent called with:', eventId);
  return this.http.get<any>(`${this.apiUrl}/${eventId}`);
}

}
