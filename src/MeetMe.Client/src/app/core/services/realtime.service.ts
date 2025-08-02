import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { filter, map } from 'rxjs/operators';

export interface RealTimeEvent {
  type: string;
  data: any;
  timestamp: Date;
  userId?: string;
}

export interface MeetingUpdate {
  meetingId: string;
  type: 'created' | 'updated' | 'deleted' | 'new_attendee' | 'attendee_left';
  data: any;
  userId: string;
}

export interface NotificationUpdate {
  id: string;
  type: 'meeting_invite' | 'meeting_reminder' | 'meeting_cancelled' | 'new_comment' | 'new_post';
  title: string;
  message: string;
  userId: string;
  data?: any;
}

export interface UserActivityUpdate {
  userId: string;
  type: 'online' | 'offline' | 'typing' | 'viewing_meeting';
  data?: any;
}

@Injectable({
  providedIn: 'root'
})
export class RealTimeService {
  private ws: WebSocket | null = null;
  private isConnected = new BehaviorSubject<boolean>(false);
  private events = new Subject<RealTimeEvent>();
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectDelay = 1000;

  constructor() {
    this.connect();
  }

  get isConnected$(): Observable<boolean> {
    return this.isConnected.asObservable();
  }

  get events$(): Observable<RealTimeEvent> {
    return this.events.asObservable();
  }

  /**
   * Connect to WebSocket server
   */
  private connect(): void {
    try {
      // In production, this would be wss://your-domain.com/ws
      this.ws = new WebSocket('ws://localhost:5000/ws');

      this.ws.onopen = () => {
        console.log('WebSocket connected');
        this.isConnected.next(true);
        this.reconnectAttempts = 0;

        // Send authentication token
        const token = localStorage.getItem('token');
        if (token) {
          this.send('authenticate', { token });
        }
      };

      this.ws.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data);
          const realTimeEvent: RealTimeEvent = {
            type: data.type,
            data: data.data,
            timestamp: new Date(data.timestamp),
            userId: data.userId
          };
          this.events.next(realTimeEvent);
        } catch (error) {
          console.error('Error parsing WebSocket message:', error);
        }
      };

      this.ws.onclose = (event) => {
        console.log('WebSocket disconnected:', event.code, event.reason);
        this.isConnected.next(false);
        this.handleReconnect();
      };

      this.ws.onerror = (error) => {
        console.error('WebSocket error:', error);
      };

    } catch (error) {
      console.error('Error connecting to WebSocket:', error);
      this.handleReconnect();
    }
  }

  /**
   * Handle reconnection logic
   */
  private handleReconnect(): void {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++;
      const delay = this.reconnectDelay * Math.pow(2, this.reconnectAttempts - 1);

      console.log(`Attempting to reconnect in ${delay}ms (attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts})`);

      setTimeout(() => {
        this.connect();
      }, delay);
    } else {
      console.error('Max reconnection attempts reached');
    }
  }

  /**
   * Send a message through WebSocket
   */
  private send(type: string, data: any): void {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify({
        type,
        data,
        timestamp: new Date().toISOString()
      }));
    }
  }

  /**
   * Subscribe to specific event types
   */
  onEvent<T = any>(eventType: string): Observable<T> {
    return this.events$.pipe(
      filter(event => event.type === eventType),
      map(event => event.data as T)
    );
  }

  /**
   * Subscribe to meeting updates
   */
  onMeetingUpdate(): Observable<MeetingUpdate> {
    return this.onEvent<MeetingUpdate>('meeting_update');
  }

  /**
   * Subscribe to notification updates
   */
  onNotificationUpdate(): Observable<NotificationUpdate> {
    return this.onEvent<NotificationUpdate>('notification');
  }

  /**
   * Subscribe to user activity updates
   */
  onUserActivity(): Observable<UserActivityUpdate> {
    return this.onEvent<UserActivityUpdate>('user_activity');
  }

  /**
   * Join a meeting room for real-time updates
   */
  joinMeetingRoom(meetingId: string): void {
    this.send('join_meeting', { meetingId });
  }

  /**
   * Leave a meeting room
   */
  leaveMeetingRoom(meetingId: string): void {
    this.send('leave_meeting', { meetingId });
  }

  /**
   * Send typing indicator
   */
  sendTyping(meetingId: string, isTyping: boolean): void {
    this.send('typing', { meetingId, isTyping });
  }

  /**
   * Update user activity status
   */
  updateActivity(status: 'online' | 'offline' | 'away'): void {
    this.send('activity_status', { status });
  }

  /**
   * Send user location for meeting proximity features
   */
  updateLocation(latitude: number, longitude: number): void {
    this.send('location_update', { latitude, longitude });
  }

  /**
   * Disconnect WebSocket
   */
  disconnect(): void {
    if (this.ws) {
      this.ws.close();
      this.ws = null;
      this.isConnected.next(false);
    }
  }

  /**
   * Get connection status
   */
  getConnectionStatus(): boolean {
    return this.isConnected.value;
  }

  /**
   * Force reconnection
   */
  forceReconnect(): void {
    this.disconnect();
    this.reconnectAttempts = 0;
    this.connect();
  }
}
