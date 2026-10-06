import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  PagedResult,
  Product,
  ProductQueryRequest
} from '../models/product.model';

import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrl}/Product`;

  /**
   * Gets paged products from the backend API.
   */
  getPaged(
    request: ProductQueryRequest
  ): Observable<PagedResult<Product>> {
    let params = new HttpParams()
      .set('page', request.page)
      .set('pageSize', request.pageSize);

    if (request.search) {
      params = params.set('search', request.search);
    }

    if (request.categoryId !== undefined) {
      params = params.set('categoryId', request.categoryId);
    }

    if (request.isActive !== undefined) {
      params = params.set('isActive', request.isActive);
    }

    if (request.sortBy) {
      params = params.set('sortBy', request.sortBy);
    }

    if (request.sortOrder) {
      params = params.set('sortOrder', request.sortOrder);
    }

    return this.http.get<PagedResult<Product>>(
      `${this.apiUrl}/paged`,
      { params }
    );
  }
}