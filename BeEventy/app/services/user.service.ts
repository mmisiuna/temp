import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, map, Observable, tap } from 'rxjs';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  userId: number;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = 'http://localhost:5260/api/account';
  currentUserEmail = new BehaviorSubject<string | null>(null);
  currentUserEmail$ = this.currentUserEmail.asObservable(); 
  currentUserImage = new BehaviorSubject<string | null>(null);
  currentUserImage$ = this.currentUserImage.asObservable();
  constructor(private http: HttpClient) {}

  getUserEmailById(userId: number): Observable<string> {
    return this.http.get<string>(`${this.apiUrl}/emailbyid/${userId}`);
  }
  getAccountByEmail(email: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/email/${email}`);
  }
  
  register(account: { name: string; email: string; password: string; phoneNumber: string }): Observable<any> {
    return this.http.post<any>(this.apiUrl, account);
  }
  getCurrentUserEmail(): Observable<string | null> {
    return this.currentUserEmail$;
  }
  emailExists(email: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/exists/email/${email}`);
  }
  getProfileImage(userId: number): Observable<string> {
    return this.http.get(`${this.apiUrl}/profileimage/${userId}`, { responseType: 'text' });
  }
  
  
  phoneNumberExists(phoneNumber: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/exists/phone/${phoneNumber}`);
  }
  login(email: string, password: string): Observable<LoginResponse>{
    const loginRequest: LoginRequest = { email, password };
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, loginRequest)
    .pipe(
      map(response => {
        if (response.token) {
          localStorage.setItem('token', response.token);
          localStorage.setItem('userId', response.userId.toString());
          this.currentUserEmail.next(email);
          this.getProfileImage(response.userId).subscribe(imageUrl => {
            this.currentUserImage.next(`assets/users/${imageUrl}`);
          });
        }
        return response;
      })
    );
  }
}
