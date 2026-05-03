import { create } from 'zustand';
import { Customer } from '@/types/crm';
import { Product } from '@/types/inventory';

export interface CartItem {
    id: string; // productVariantId or unique combination
    productId: string;
    productName: string;
    variantId: string;
    variantName: string;
    price: number;
    quantity: number;
    maxStock: number;
    discount: number;
    isVatExcluded?: boolean;
}

interface PosState {
    cart: CartItem[];
    selectedCustomer: Customer | null;
    refreshKey: number;
    activeVats: { name: string; rate: number }[];
    vatCalculationType: number; // 1 = Exclusive, 2 = Inclusive

    setVats: (vats: { name: string; rate: number }[]) => void;
    setVatCalculationType: (type: number) => void;
    addToCart: (product: Product, variantId?: string) => void;
    removeFromCart: (variantId: string) => void;
    updateQuantity: (variantId: string, quantity: number) => void;
    setDiscount: (variantId: string, discount: number) => void;
    setCustomer: (customer: Customer | null) => void;
    clearCart: () => void;
    triggerRefresh: () => void;

    // Computed totals helper
    getTotals: () => {
        subtotal: number;
        tax: number;
        discount: number;
        total: number;
    };
}

export const usePosStore = create<PosState>((set, get) => ({
    cart: [],
    selectedCustomer: null,
    refreshKey: 0,
    activeVats: [],
    vatCalculationType: 1,

    setVats: (vats: { name: string; rate: number }[]) => {
        set({ activeVats: vats });
    },

    setVatCalculationType: (type: number) => {
        set({ vatCalculationType: type });
    },

    addToCart: (product: Product, variantId?: string) => {
        set((state) => {
            // Logic to find variant
            let variant: any = null;
            if (variantId) {
                variant = product.variants?.find(v => v.variantId === variantId);
            } else if (product.variants && product.variants.length > 0) {
                // Default to first if none specified
                variant = product.variants[0];
            }

            if (!variant) return state; // Should alert or handle error, but for now safe return

            // Check stock logic (optional, UI usually handles this but good for safety)
            // if (variant.stockLevel <= 0) return state; 

            const cartId = variant.variantId || product.productId;
            const existingItem = state.cart.find(item => item.variantId === cartId);

            if (existingItem) {
                if (existingItem.quantity >= variant.stockLevel) {
                    return state; // Stock limit reached
                }

                return {
                    cart: state.cart.map(item =>
                        item.variantId === cartId
                            ? { ...item, quantity: item.quantity + 1 }
                            : item
                    )
                };
            }

            const newItem: CartItem = {
                id: cartId,
                productId: product.productId!,
                productName: product.productName,
                variantId: cartId,
                variantName: variant.variantName,
                price: variant.price,
                quantity: 1,
                maxStock: variant.stockLevel,
                discount: 0,
                isVatExcluded: product.isVatExcluded
            };

            return { cart: [...state.cart, newItem] };
        });
    },

    removeFromCart: (variantId: string) => {
        set((state) => ({
            cart: state.cart.filter(item => item.variantId !== variantId)
        }));
    },

    updateQuantity: (variantId: string, quantity: number) => {
        set((state) => ({
            cart: state.cart.map(item => {
                if (item.variantId === variantId) {
                    // Enforce max stock
                    const newQty = Math.min(Math.max(1, quantity), item.maxStock);
                    return { ...item, quantity: newQty };
                }
                return item;
            })
        }));
    },

    setDiscount: (variantId: string, discount: number) => {
        set((state) => ({
            cart: state.cart.map(item =>
                item.variantId === variantId
                    ? { ...item, discount: discount }
                    : item
            )
        }));
    },

    setCustomer: (customer: Customer | null) => {
        set({ selectedCustomer: customer });
    },

    clearCart: () => {
        set({ cart: [], selectedCustomer: null });
    },

    triggerRefresh: () => {
        set((state) => ({ refreshKey: state.refreshKey + 1 }));
    },

    getTotals: () => {
        const { cart, activeVats, vatCalculationType } = get();

        let subtotal = 0;
        let discount = 0;
        let tax = 0;
        let total = 0;

        const totalVatRate = activeVats && activeVats.length > 0
            ? activeVats.reduce((sum, vat) => sum + vat.rate, 0)
            : 0;

        cart.forEach((item) => {
            const itemDiscount = item.discount || 0;
            discount += itemDiscount;

            const lineTotal = (item.price * item.quantity) - itemDiscount;

            if (!item.isVatExcluded && activeVats && activeVats.length > 0) {
                if (vatCalculationType === 1) {
                    // Exclusive: Tax is added on top
                    let itemTax = 0;
                    activeVats.forEach(v => {
                        itemTax += lineTotal * (v.rate / 100);
                    });
                    tax += itemTax;
                    subtotal += (item.price * item.quantity);
                    total += lineTotal + itemTax;
                } else if (vatCalculationType === 2) {
                    // Inclusive: Tax is extracted from the price
                    const baseLineTotal = lineTotal / (1 + (totalVatRate / 100));
                    let itemTax = 0;
                    activeVats.forEach(v => {
                        itemTax += baseLineTotal * (v.rate / 100);
                    });
                    tax += itemTax;
                    subtotal += baseLineTotal + itemDiscount; // Subtotal becomes the pre-tax amounts
                    total += lineTotal;
                }
            } else {
                // No VAT or Excluded
                subtotal += (item.price * item.quantity);
                total += lineTotal;
            }
        });

        return { subtotal, discount, tax, total };
    }
}));
