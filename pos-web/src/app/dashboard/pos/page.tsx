import { ProductGrid } from "./components/product-grid";
import { CartSidebar } from "./components/cart-sidebar";
import { Metadata } from 'next';

export const metadata: Metadata = {
    title: 'Point of Sale | POS System',
    description: 'Process sales transactions',
};

export default function PosPage() {
    return (
        <div className="flex h-[calc(100vh-4rem)] overflow-hidden">
            {/* 4rem is approx heater height. Adjust if needed to fit viewport without double scrollbars */}

            {/* Left Panel: Product Catalog */}
            <div className="flex-1 p-4 bg-background overflow-hidden flex flex-col min-w-0">
                <ProductGrid />
            </div>

            {/* Right Panel: Cart */}
            <div className="w-[400px] shrink-0 border-l bg-card h-full flex flex-col">
                <CartSidebar />
            </div>
        </div>
    );
}
