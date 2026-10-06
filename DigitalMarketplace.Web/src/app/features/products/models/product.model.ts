export interface Product {
  id: number;
  categoryId: number;
  categoryName: string;
  name: string;
  slug: string;
  description: string | null;
  price: number;
  stockQuantity: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface ProductQueryRequest {
  search?: string;
  categoryId?: number;
  isActive?: boolean;
  page: number;
  pageSize: number;
  sortBy?: string;
  sortOrder?: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}