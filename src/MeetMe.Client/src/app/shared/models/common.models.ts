export interface ApiResponse<T> {
  isSuccess: boolean;
  value?: T;
  error?: string;
}

export interface SearchFilters {
  query?: string;
  location?: string;
  startDate?: string;
  endDate?: string;
  isPublic?: boolean;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface ErrorResponse {
  message: string;
  errors?: { [key: string]: string[] };
}
