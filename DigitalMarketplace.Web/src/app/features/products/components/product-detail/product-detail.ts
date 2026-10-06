import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

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
  description: string;
  stock: number;
}

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.css'
})
export class ProductDetail {

  private readonly route = inject(ActivatedRoute);

  productSlug = '';

  product: Product | null = null;

  private readonly products: Product[] = [
    {
      id: 1,
      name: 'Power Strike 900',
      category: 'Vợt cầu lông',
      price: 1890000,
      oldPrice: 2190000,
      rating: 4.8,
      reviews: 126,
      image: 'assets/products/racket-01.jpg',
      badge: 'Bán chạy',
      description:
        'Vợt cầu lông Power Strike 900 được thiết kế dành cho người chơi yêu thích lối đánh tấn công, cân bằng giữa sức mạnh và khả năng kiểm soát.',
      stock: 18
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
      badge: 'Hot',
      description:
        'Thunder Pro 88 mang lại khả năng phản tạt nhanh và cảm giác cầu ổn định, phù hợp cho người chơi có trình độ trung bình đến nâng cao.',
      stock: 12
    },

    {
      id: 3,
      name: 'Speed Court X',
      category: 'Giày cầu lông',
      price: 1590000,
      rating: 4.7,
      reviews: 74,
      image: 'assets/products/shoes-01.jpg',
      badge: 'Mới',
      description:
        'Giày Speed Court X được thiết kế với trọng lượng nhẹ, hỗ trợ di chuyển nhanh và tăng độ ổn định khi thi đấu trên sân.',
      stock: 25
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
      badge: 'Sale',
      description:
        'Áo Pro Match Jersey sử dụng chất liệu nhẹ và thoáng khí, phù hợp cho tập luyện và thi đấu cầu lông.',
      stock: 32
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
      badge: 'Hot',
      description:
        'Ultra Power 100 hướng đến khả năng tạo lực mạnh và độ ổn định cao trong các pha tấn công.',
      stock: 9
    },

    {
      id: 6,
      name: 'Court Master Pro',
      category: 'Giày cầu lông',
      price: 1890000,
      rating: 4.8,
      reviews: 63,
      image: 'assets/products/shoes-02.jpg',
      description:
        'Court Master Pro hỗ trợ tốt cho các pha di chuyển ngang và chuyển hướng nhanh trên sân cầu lông.',
      stock: 16
    }
  ];

  constructor() {
    this.productSlug =
      this.route.snapshot.paramMap.get('slug') ?? '';

    this.loadProduct();
  }

  /**
   * Finds a product from mock data using the URL slug.
   */
  private loadProduct(): void {
    const slug = this.productSlug;

    this.product =
      this.products.find(
        product =>
          this.createSlug(product.name) === slug
      ) ?? null;
  }

  /**
   * Converts a product name into a URL-friendly slug.
   */
  private createSlug(value: string): string {
    return value
      .toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/đ/g, 'd')
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '');
  }

  /**
   * Formats a price using Vietnamese currency format.
   */
  formatPrice(price: number): string {
    return new Intl.NumberFormat(
      'vi-VN'
    ).format(price) + ' ₫';
  }

  /**
   * Calculates the discount percentage.
   */
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

  /**
   * Returns the rating stars.
   */
  getStars(rating: number): string {
    return '★'.repeat(
      Math.round(rating)
    );
  }
}