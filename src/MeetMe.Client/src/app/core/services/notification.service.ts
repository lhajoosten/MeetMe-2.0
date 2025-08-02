import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Notification {
  id: string;
  type: 'info' | 'success' | 'warning' | 'error';
  title: string;
  message: string;
  timestamp: Date;
  isRead: boolean;
  actionUrl?: string;
  actionText?: string;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notifications$ = new BehaviorSubject<Notification[]>([]);
  private unreadCount$ = new BehaviorSubject<number>(0);

  constructor() {}

  getNotifications(): Observable<Notification[]> {
    return this.notifications$.asObservable();
  }

  getUnreadCount(): Observable<number> {
    return this.unreadCount$.asObservable();
  }

  addNotification(notification: Omit<Notification, 'id' | 'timestamp' | 'isRead'>): void {
    const newNotification: Notification = {
      ...notification,
      id: this.generateId(),
      timestamp: new Date(),
      isRead: false
    };

    const current = this.notifications$.value;
    this.notifications$.next([newNotification, ...current]);
    this.updateUnreadCount();
  }

  markAsRead(notificationId: string): void {
    const current = this.notifications$.value;
    const updated = current.map(notification =>
      notification.id === notificationId
        ? { ...notification, isRead: true }
        : notification
    );
    this.notifications$.next(updated);
    this.updateUnreadCount();
  }

  markAllAsRead(): void {
    const current = this.notifications$.value;
    const updated = current.map(notification => ({ ...notification, isRead: true }));
    this.notifications$.next(updated);
    this.updateUnreadCount();
  }

  removeNotification(notificationId: string): void {
    const current = this.notifications$.value;
    const updated = current.filter(notification => notification.id !== notificationId);
    this.notifications$.next(updated);
    this.updateUnreadCount();
  }

  clearAll(): void {
    this.notifications$.next([]);
    this.updateUnreadCount();
  }

  // Helper methods for common notification types
  showSuccess(title: string, message: string, actionUrl?: string, actionText?: string): void {
    this.addNotification({
      type: 'success',
      title,
      message,
      actionUrl,
      actionText
    });
  }

  showError(title: string, message: string): void {
    this.addNotification({
      type: 'error',
      title,
      message
    });
  }

  showInfo(title: string, message: string, actionUrl?: string, actionText?: string): void {
    this.addNotification({
      type: 'info',
      title,
      message,
      actionUrl,
      actionText
    });
  }

  showWarning(title: string, message: string): void {
    this.addNotification({
      type: 'warning',
      title,
      message
    });
  }

  private updateUnreadCount(): void {
    const unread = this.notifications$.value.filter(n => !n.isRead).length;
    this.unreadCount$.next(unread);
  }

  private generateId(): string {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
  }
}
