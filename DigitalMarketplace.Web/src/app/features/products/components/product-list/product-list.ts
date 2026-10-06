import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

interface Product {
  id: number;
  name: string;
  category: string;
  price: number;
  oldPrice?: number;
  rating: number;
  reviews: number;
  image: string;
  badge?: string;
}

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink
  ],
  templateUrl: './product-list.html',
  styleUrl: './product-list.css'
})
export class ProductList {

  categories: string[] = [
    'Tất cả',
    'Vợt cầu lông',
    'Giày cầu lông',
    'Quần áo cầu lông',
    'Cầu lông',
    'Túi cầu lông',
    'Phụ kiện'
  ];

  selectedCategory = 'Tất cả';

  searchKeyword = '';

  sortOption = 'newest';

  currentPage = 1;

  pageSize = 8;

  products: Product[] = [

    {
      id: 1,
      name: 'Power Strike 900',
      category: 'Vợt cầu lông',
      price: 1890000,
      oldPrice: 2190000,
      rating: 4.8,
      reviews: 126,
      image: 'assets/products/racket-01.jpg',
      badge: 'Bán chạy'
    },

    {
      id: 2,
      name: 'Thunder Pro 88',
      category: 'Vợt cầu lông',
      price: 2290000,
      oldPrice: 2590000,
      rating: 4.9,
      reviews: 98,
      image: 'assets/products/racket-02.jpg',
      badge: 'Hot'
    },

    {
      id: 3,
      name: 'Speed Court X',
      category: 'Giày cầu lông',
      price: 1590000,
      rating: 4.7,
      reviews: 74,
      image: 'assets/products/shoes-01.jpg',
      badge: 'Mới'
    },

    {
      id: 4,
      name: 'Pro Match Jersey',
      category: 'Quần áo cầu lông',
      price: 690000,
      oldPrice: 790000,
      rating: 4.6,
      reviews: 51,
      image: 'assets/products/jersey-01.jpg',
      badge: 'Sale'
    },

    {
      id: 5,
      name: 'Ultra Power 100',
      category: 'Vợt cầu lông',
      price: 2790000,
      oldPrice: 3190000,
      rating: 4.9,
      reviews: 86,
      image: 'assets/products/racket-03.jpg',
      badge: 'Hot'
    },

    {
      id: 6,
      name: 'Court Master Pro',
      category: 'Giày cầu lông',
      price: 1890000,
      rating: 4.8,
      reviews: 63,
      image: 'assets/products/shoes-02.jpg'
    },

    {
      id: 7,
      name: 'Elite Training Tee',
      category: 'Quần áo cầu lông',
      price: 490000,
      oldPrice: 590000,
      rating: 4.5,
      reviews: 42,
      image: 'assets/products/jersey-02.jpg',
      badge: 'Sale'
    },

    {
      id: 8,
      name: 'Tournament Shuttle Pro',
      category: 'Cầu lông',
      price: 420000,
      rating: 4.8,
      reviews: 117,
      image: 'assets/products/shuttle-01.jpg',
      badge: 'Bán chạy'
    },

    {
      id: 9,
      name: 'Carbon Elite 77',
      category: 'Vợt cầu lông',
      price: 2490000,
      rating: 4.7,
      reviews: 55,
      image: 'assets/products/racket-04.jpg'
    },

    {
      id: 10,
      name: 'Speed Runner',
      category: 'Giày cầu lông',
      price: 1290000,
      oldPrice: 1490000,
      rating: 4.6,
      reviews: 39,
      image: 'assets/products/shoes-03.jpg',
      badge: 'Sale'
    },

    {
      id: 11,
      name: 'Pro Team Shorts',
      category: 'Quần áo cầu lông',
      price: 390000,
      rating: 4.5,
      reviews: 31,
      image: 'assets/products/shorts-01.jpg'
    },

    {
      id: 12,
      name: 'HLT Premium Bag',
      category: 'Túi cầu lông',
      price: 990000,
      oldPrice: 1190000,
      rating: 4.8,
      reviews: 48,
      image: 'assets/products/bag-01.jpg',
      badge: 'Mới'
    },

    {
      id: 13,
      name: 'Pro Grip X',
      category: 'Phụ kiện',
      price: 120000,
      rating: 4.7,
      reviews: 91,
      image: 'assets/products/grip-01.jpg'
    },

    {
      id: 14,
      name: 'Power String 0.68',
      category: 'Phụ kiện',
      price: 180000,
      rating: 4.8,
      reviews: 67,
      image: 'assets/products/string-01.jpg'
    },

    {
      id: 15,
      name: 'Tournament Net',
      category: 'Phụ kiện',
      price: 690000,
      rating: 4.6,
      reviews: 22,
      image: 'assets/products/net-01.jpg'
    },

    {
      id: 16,
      name: 'Carbon Power 99',
      category: 'Vợt cầu lông',
      price: 3190000,
      oldPrice: 3590000,
      rating: 4.9,
      reviews: 73,
      image: 'assets/products/racket-05.jpg',
      badge: 'Hot'
    }
  ];

  get filteredProducts(): Product[] {
    let result = [...this.products];

    if (this.selectedCategory !== 'Tất cả') {
      result = result.filter(
        product =>
          product.category === this.selectedCategory
      );
    }

    const keyword = this.searchKeyword
      .trim()
      .toLowerCase();

    if (keyword) {
      result = result.filter(product =>
        product.name.toLowerCase().includes(keyword)
        ||
        product.category.toLowerCase().includes(keyword)
      );
    }

    switch (this.sortOption) {

      case 'priceAsc':
        result.sort(
          (a, b) => a.price - b.price
        );
        break;

      case 'priceDesc':
        result.sort(
          (a, b) => b.price - a.price
        );
        break;

      case 'rating':
        result.sort(
          (a, b) => b.rating - a.rating
        );
        break;

      case 'newest':
      default:
        result.sort(
          (a, b) => b.id - a.id
        );
        break;
    }

    return result;
  }

  get totalPages(): number {
    return Math.ceil(
      this.filteredProducts.length / this.pageSize
    );
  }

  get pagedProducts(): Product[] {
    const start =
      (this.currentPage - 1) * this.pageSize;

    return this.filteredProducts.slice(
      start,
      start + this.pageSize
    );
  }

  get paginationPages(): number[] {
    return Array.from(
      { length: this.totalPages },
      (_, index) => index + 1
    );
  }

  selectCategory(category: string): void {
    this.selectedCategory = category;
    this.currentPage = 1;
  }

  onSearch(): void {
    this.currentPage = 1;
  }

  onSortChange(): void {
    this.currentPage = 1;
  }

  changePage(page: number): void {

    if (
      page < 1 ||
      page > this.totalPages ||
      page === this.currentPage
    ) {
      return;
    }

    this.currentPage = page;

    window.scrollTo({
      top: 0,
      behavior: 'smooth'
    });
  }

  previousPage(): void {
    this.changePage(
      this.currentPage - 1
    );
  }

  nextPage(): void {
    this.changePage(
      this.currentPage + 1
    );
  }

  addToCart(product: Product): void {
    console.log(
      'Add product to cart:',
      product
    );
  }

  viewProduct(product: Product): void {
    console.log(
      'View product:',
      product
    );
  }

  toggleWishlist(product: Product): void {
    console.log(
      'Toggle wishlist:',
      product
    );
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat(
      'vi-VN'
    ).format(price) + ' ₫';
  }

  getDiscountPercent(product: Product): number {

    if (!product.oldPrice) {
      return 0;
    }

    return Math.round(
      (
        (product.oldPrice - product.price)
        /
        product.oldPrice
      ) * 100
    );
  }

  getStars(rating: number): string {
    return '★'.repeat(
      Math.round(rating)
    );
  }
}