import {
  ChartPieIcon,
  DocumentChartBarIcon,
  PhotoIcon,
  TableCellsIcon,
} from '@heroicons/react/24/outline'
import type { ComponentType, SVGProps } from 'react'

export type NavRoute = {
  path: string
  label: string
  icon: ComponentType<SVGProps<SVGSVGElement>>
}

export const NAV_ROUTES: NavRoute[] = [
  { path: '/', label: 'Dashboard', icon: ChartPieIcon },
  { path: '/transactions', label: 'Transactions', icon: TableCellsIcon },
  { path: '/receipts', label: 'Receipts', icon: PhotoIcon },
  { path: '/reports', label: 'Reports', icon: DocumentChartBarIcon },
]
