import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface Category {
  name: string;
  slug: string;
  icon: string;
  productCount: number;
}

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
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home {

  categories: Category[] = [
    {
      name: 'Vợt cầu lông',
      slug: 'vot-cau-long',
      icon: '🏸',
      productCount: 48
    },
    {
      name: 'Giày cầu lông',
      slug: 'giay-cau-long',
      icon: '👟',
      productCount: 32
    },
    {
      name: 'Quần áo',
      slug: 'quan-ao-cau-long',
      icon: '👕',
      productCount: 26
    },
    {
      name: 'Cầu lông',
      slug: 'cau-long',
      icon: '🏸',
      productCount: 18
    },
    {
      name: 'Túi cầu lông',
      slug: 'tui-cau-long',
      icon: '🎒',
      productCount: 21
    },
    {
      name: 'Phụ kiện',
      slug: 'phu-kien',
      icon: '🎽',
      productCount: 35
    }
  ];

  featuredProducts: Product[] = [
    {
      id: 1,
      name: 'Power Strike 900',
      category: 'Vợt cầu lông',
      price: 1890000,
      oldPrice: 2190000,
      rating: 4.8,
      reviews: 126,
      image: 'https://placehold.co/600x600/f3f4f6/111827?text=RACKET+900',
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
      image: 'https://placehold.co/600x600/f3f4f6/111827?text=RACKET+88',
      badge: 'Hot'
    },
    {
      id: 3,
      name: 'Speed Court X',
      category: 'Giày cầu lông',
      price: 1590000,
      rating: 4.7,
      reviews: 74,
      image: 'https://placehold.co/600x600/f3f4f6/111827?text=SHOES+X',
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
      image: 'https://placehold.co/600x600/f3f4f6/111827?text=JERSEY',
      badge: 'Sale'
    }
  ];

  /**
   * Formats product price as Vietnamese currency.
   */
  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN').format(price) + ' ₫';
  }

  /**
   * Returns the number of filled stars for a product.
   */
  getStars(rating: number): string {
    return '★'.repeat(Math.round(rating));
  }

}