export interface Meeting {
  id: string;
  title: string;
  description: string;
  startDateTime: string;
  endDateTime: string;
  location: string;
  maxAttendees?: number;
  isPublic: boolean;
  isActive: boolean;
  creatorId: string;
  creatorName: string;
  attendeeCount: number;
  postCount: number;
  isUpcoming: boolean;
  createdDate: string;
  organizer?: User;      		// Deprecated
  attendees?: Attendance[]; // Deprecated
  posts?: Post[];        		// Deprecated
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
  id: number;
  title?: string;
  content: string;
  authorId: string;
  author: User;
  meetingId: string;
  meeting: Meeting;
  comments: Comment[];
  likes: number;
  isLiked: boolean;
  isBookmarked: boolean;
  tags: string[];
  createdAt: string;
  updatedAt: string;
}

export interface Comment {
  id: number;
  content: string;
  authorId: string;
  author: User;
  postId: number;
  post: Post;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePostRequest {
  title?: string;
  content: string;
  meetingId: string;
}

export interface CreateCommentRequest {
  content: string;
  postId: number;
}

import { User } from './auth.models';
