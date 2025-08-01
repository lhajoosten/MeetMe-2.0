export interface Meeting {
  id: string;
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  location: string;
  maxAttendees?: number;
  isPublic: boolean;
  organizerId: string;
  organizer: User;
  attendees: Attendance[];
  posts: Post[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateMeetingRequest {
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  location: string;
  maxAttendees?: number;
  isPublic: boolean;
}

export interface UpdateMeetingRequest {
  id: string;
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  location: string;
  maxAttendees?: number;
  isPublic: boolean;
}

export interface Attendance {
  id: string;
  userId: string;
  user: User;
  meetingId: string;
  meeting: Meeting;
  status: AttendanceStatus;
  joinedAt: string;
}

export enum AttendanceStatus {
  Pending = 'Pending',
  Confirmed = 'Confirmed',
  Declined = 'Declined',
  Attended = 'Attended'
}

export interface Post {
  id: string;
  content: string;
  authorId: string;
  author: User;
  meetingId: string;
  meeting: Meeting;
  comments: Comment[];
  createdAt: string;
  updatedAt: string;
}

export interface Comment {
  id: string;
  content: string;
  authorId: string;
  author: User;
  postId: string;
  post: Post;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePostRequest {
  content: string;
  meetingId: string;
}

export interface CreateCommentRequest {
  content: string;
  postId: string;
}

import { User } from './auth.models';
