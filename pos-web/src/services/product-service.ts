import api from "@/lib/api";
import { Product, CreateProductRequest, Category, CreateCategoryRequest } from "@/types/inventory";

const mapDtoToProduct = (dto: any): Product => {
    // console.log("DEBUG: Mapping Product DTO", dto); // Temporary Debug
    return {
        productId: dto.id,
        productName: dto.productName,
        sku: dto.productSkuBase,
        categoryId: dto.categoryId,
        categoryName: dto.categoryName,
        supplierId: dto.supplierId, // Added mapping
        supplierName: dto.supplierName, // Added mapping
        price: dto.price,
        cost: dto.cost,
        stockLevel: dto.stockLevel,
        newStock: dto.newStock,
        minStockLevel: dto.minStockLevel,
        description: dto.description,
        image1: dto.image1,
        isActive: dto.isActive,
        tenantId: "",
        hasVariants: dto.variants && dto.variants.length > 0,
        variants: dto.variants?.map((v: any) => ({
            variantId: v.id,
            sku: v.variantSku,
            variantName: v.variantName,
            price: v.price || 0,
            stockLevel: v.stockLevel || 0,
            cost: v.cost || 0,
            minStockLevel: v.minStockLevel || 0,
            attributes: {}
        })) || []
    };
};

export const productService = {
    getProducts: async (locationId?: string) => {
        const url = locationId ? `/api/Products?locationId=${locationId}` : '/api/Products';
        const response = await api.get(url);
        return response.data.data.map(mapDtoToProduct);
    },

    getProduct: async (id: string, locationId?: string) => {
        const url = locationId ? `/api/Products/${id}?locationId=${locationId}` : `/api/Products/${id}`;
        const response = await api.get(url);
        return mapDtoToProduct(response.data.data);
    },

    createProduct: async (data: any) => {
        const payload = {
            productName: data.productName,
            productSkuBase: data.sku, // Map sku to ProductSkuBase
            categoryId: data.categoryId,
            supplierId: data.supplierId, // Added
            locationId: data.locationId, // Map locationId
            price: data.price,
            cost: data.cost,
            stockLevel: data.stockLevel,
            // newStock removed as it is not in interface
            minStockLevel: data.minStockLevel,
            description: data.description,
            image1: data.image1,
            isActive: data.isActive,
            variants: data.variants?.map((v: any) => ({
                variantName: v.variantName,
                variantSku: v.sku, // Map sku to VariantSku
                price: v.price,
                cost: v.cost,
                stockLevel: v.stockLevel,
                minStockLevel: v.minStockLevel,
                attributes: v.attributes
            })) || []
        };
        const response = await api.post('/api/Products', payload);
        return mapDtoToProduct(response.data.data);
    },

    updateProduct: async (id: string, data: Partial<CreateProductRequest>) => {
        const payload: any = {
            ...data,
            updateReason: "Manual Update" // Required by backend
        };
        if (data.sku) payload.productSkuBase = data.sku;
        if (data.variants) {
            payload.variants = data.variants.map(v => ({
                id: v.variantId || null, // Map frontend variantId to backend (null for new)
                variantName: v.variantName,
                variantSku: v.sku, // Map sku to VariantSku
                price: v.price,
                cost: v.cost,
                stockLevel: v.stockLevel,
                minStockLevel: v.minStockLevel,
                attributes: v.attributes
            }));
        }

        console.log("DEBUG: updateProduct payload:", JSON.stringify(payload, null, 2));

        const response = await api.put(`/api/Products/${id}`, payload);
        return mapDtoToProduct(response.data.data);
    },

    deleteProduct: async (id: string) => {
        await api.delete(`/api/Products/${id}`);
    },

    deleteAllProducts: async () => {
        const response = await api.delete('/api/Products/all');
        return response.data;
    },

    uploadImage: async (file: File) => {
        const formData = new FormData();
        formData.append('file', file);
        const response = await api.post('/api/Media/upload', formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });
        return response.data.data.url;
    },

    bulkUploadProducts: async (file: File) => {
        const formData = new FormData();
        formData.append("file", file);
        const response = await api.post('/api/Products/bulk-upload', formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });
        return response.data;
    },

    getProductLocations: async (productId: string): Promise<string[]> => {
        const response = await api.get(`/api/Products/${productId}/locations`);
        return response.data.data || [];
    },

    updateProductLocations: async (productId: string, locationIds: string[]): Promise<void> => {
        await api.put(`/api/Products/${productId}/locations`, locationIds);
    }
};

export const categoryService = {
    getCategories: async () => {
        const response = await api.get('/api/Categories');
        return response.data.data;
    },

    createCategory: async (data: CreateCategoryRequest) => {
        const response = await api.post('/api/Categories', data);
        return response.data.data;
    },

    updateCategory: async (id: string, data: Partial<CreateCategoryRequest>) => {
        const response = await api.put(`/api/Categories/${id}`, data);
        return response.data.data;
    },

    deleteCategory: async (id: string) => {
        await api.delete(`/api/Categories/${id}`);
    }
};
